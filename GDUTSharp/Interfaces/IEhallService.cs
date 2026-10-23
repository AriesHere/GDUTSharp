using System.Net;
using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>融合门户</summary>
/// <remarks>
/// <para>
/// 先通过 <see cref="Login(CookieContainer, LoginInfo?, CancellationToken)"/> 登录（统
/// 一认证中心登录），然后调用 <see cref="GetCardWid(CookieContainer, CancellationToken)"/>
/// 获得 cardWid，再用它经由 <see cref="GetWid(CookieContainer, string, CancellationToken)"/>
/// 获取校园网和校园卡的 wid，再根据实际情况使用二者获得数据
/// </para>
/// <para>
/// 注意：wid 和 cardWid 是两个不同的东西。另外暂时不要使用 <see cref="GetCampusCardInfo"/> 和
/// <see cref="GetCampusNetworkInfo"/>，不知为何，它们的返回值始终为 {"errcode":"0","errmsg":"请求成功","data":null}
/// </para>
/// </remarks>
public interface IEhallService
{
    /// <summary>通过统一认证中心登录</summary>
    public Task<bool> Login(CookieContainer cookies, LoginInfo? loginInfo = null, CancellationToken token = default);

    public Task<string?> GetCardWid(CookieContainer cookies, CancellationToken token = default);

    /// <summary>获取请求校园卡和校园网信息时所需的 wid 参数</summary>
    public Task<(string CampusCardWid, string CampusNetwork)?> GetWid(CookieContainer cookies, string cardWid, CancellationToken token = default);

    /// <summary>获取校园卡余额和卡号</summary>
    public Task<(decimal Balance, string CardId)?> GetCampusCardInfo(CookieContainer cookies, string cardWid, string wid, CancellationToken token = default);

    /// <summary>获取校园网余额和剩余月数</summary>
    public Task<(decimal Balance, int RemainingMonths)?> GetCampusNetworkInfo(CookieContainer cookies, string cardWid, string wid, CancellationToken token = default);
}
