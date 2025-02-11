﻿using MMORPG.Common.Network;
using MMORPG.Common.Proto.Base;
using GameServer.Db;
using GameServer.Network;
using GameServer.Tool;
using Serilog;
using MMORPG.Common.Proto.User;
using GameServer.UserSystem;
using GameServer.Manager;

namespace GameServer.NetService
{
    // 可能有逻辑仍需要加锁
    public class UserService : ServiceBase<UserService>
    {
        private static readonly object _loginLock = new();
        private static readonly object _registerLock = new();

        public void OnChannelClosed(NetChannel sender)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                if (sender.User == null)
                    return;

                if (sender.User.Player != null)
                {
                    sender.User.Player.Valid = false;
                }

                UserManager.Instance.RemoveUser(sender.User.DbUser.Username);
            });
        }

        // TODO:校验用户名、密码的合法性(长度等)
        public void OnHandle(NetChannel sender, LoginRequest request)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                Log.Debug($"{sender}登录请求: Username={request.Username}, Password={request.Password}");
                
                if (sender.User != null)
                {
                    sender.Send(new LoginResponse() { Error = NetError.UnknowError });
                    Log.Debug($"{sender}登录失败：用户已登录");
                    return;
                }

                if (UserManager.Instance.GetUserByName(request.Username) != null)
                {
                    sender.Send(new LoginResponse() { Error = NetError.LoginConflict });
                    Log.Debug($"{sender}登录失败：账号已在别处登录");
                    return;
                }

                var dbUser = SqlDb.FreeSql().Select<DbUser>()
                    .Where(p => p.Username == request.Username)
                    .Where(p => p.Password == request.Password)
                    .First();
                if (dbUser == null)
                {
                    sender.Send(new LoginResponse() { Error = NetError.IncorrectUsernameOrPassword });
                    Log.Debug($"{sender}登录失败：账号或密码错误");
                    return;
                }

                sender.SetUser(UserManager.Instance.NewUser(sender, dbUser));
            

                sender.Send(new LoginResponse() { Error = NetError.Success });
                Log.Debug($"{sender}登录成功");
            });
        }

        public void OnHandle(NetChannel sender, RegisterRequest request)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                Log.Debug($"{sender}注册请求: Username={request.Username}, Password={request.Password}");
                if (sender.User != null)
                {
                    sender.Send(new RegisterResponse() { Error = NetError.UnknowError });
                    Log.Debug($"{sender}注册失败：用户已登录");
                    return;
                }

                if (!StringHelper.NameVerify(request.Username))
                {
                    sender.Send(new RegisterResponse() { Error = NetError.IllegalUsername });
                    Log.Debug($"{sender}注册失败：用户名非法");
                    return;
                }

                var dbUser = SqlDb.FreeSql().Select<DbUser>()
                    .Where(p => p.Username == request.Username)
                    .First();
                if (dbUser != null)
                {
                    sender.Send(new RegisterResponse() { Error = NetError.RepeatUsername });
                    Log.Debug($"{sender}注册失败：用户名已被注册");
                    return;
                }

                var newDbUser = new DbUser(request.Username, request.Password, Authoritys.Player);
                var insertCount = SqlDb.FreeSql().Insert<DbUser>(newDbUser).ExecuteAffrows();
                if (insertCount <= 0)
                {
                    sender.Send(new RegisterResponse() { Error = NetError.UnknowError });
                    Log.Debug($"{sender}注册失败：数据库错误");
                    return;
                }

                sender.Send(new RegisterResponse() { Error = NetError.Success });
                Log.Debug($"{sender}注册成功");
                
            });
        }

        public void OnHandle(NetChannel sender, HeartBeatRequest request)
        {
            UpdateManager.Instance.AddTask(() =>
            {
                Log.Debug($"{sender}发送心跳请求");
                //sender.Send(new HeartBeatResponse() { }, null);
            });
        }

        public void OnConnect(NetChannel sender)
        {
        }
    }
}
