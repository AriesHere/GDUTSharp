using System.Net;
using System.Net.Http.Headers;
using System.Text;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Attributes;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

[RegisterToDI(nameof(IEhallService))]
public class EhallService(ILogger<EhallService> logger, ICommonClient client, IAuthService auth) : IEhallService
{
    protected ILogger<EhallService> _logger = logger;
    protected ICommonClient _client = client;
    protected IAuthService _auth = auth;

    public virtual async Task<bool> Login(CookieContainer cookies, LoginInfo? loginInfo = null, CancellationToken token = default)
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            request = new HttpRequestMessage(HttpMethod.Get, GSConst.EhallIndex);
            response = await _client.SendAsync(cookies, request, true, token);
            request.Dispose();
            if ((await response.Content.ReadAsStringAsync(token)).StartsWith("<!DOCTYPE html><html lang=\"zh\" id=\"html-content\">"))
                return true;
            response.Dispose();
            response = await _auth.LoginAndAuth(cookies, AuthServices.Ehall, loginInfo, token);
            if (response is null) return false;
            return (await response.Content.ReadAsStringAsync(token)).StartsWith("<!DOCTYPE html><html lang=\"zh\" id=\"html-content\">");
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "登录", e);
            return false;
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
    }

    public virtual async Task<string?> GetCardWid(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GSConst.EhallPageView);
            using var response = await _client.SendAsync(cookies, request, false, token);
            var json = await response.Content.ReadAsStringAsync(token);
            return json.LastExtract("\\\"cardWid\\\":\\\"", "\\\",\\\"showTitle\\\":false,\\\"layoutCardTitle\\\":{\\\"cardTitle\\\":\\\"个人事务\\\"", out _);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "获取 cardWid", e);
            return null;
        }
    }

    public virtual async Task<(string CampusCardWid, string CampusNetwork)?> GetWid(CookieContainer cookies, string cardWid, CancellationToken token = default)
    {
        try
        {
            // 原始 json：{"method":"getPersonalDataList"}
            var content = "{\"method\":\"getPersonalDataList\"}";
            using var request = new HttpRequestMessage(HttpMethod.Post, string.Format(GSConst.EhallGetData, cardWid))
            {
                Content = new StringContent(content, Encoding.UTF8, new MediaTypeHeaderValue("application/json")),
            };
            using var response = await _client.SendAsync(cookies, request, false, token);
            var json = await response.Content.ReadAsStringAsync(token);
            return (json.LastExtract("\"wid\":\"", "\",\"title\":\"校园卡\"", out _),
                json.LastExtract("\"wid\":\"", "\",\"title\":\"校园网续费\"", out _));
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "获取 wid", e);
            return null;
        }
    }

    public virtual async Task<(decimal Balance, string CardId)?> GetCampusCardInfo(CookieContainer cookies, string cardWid, string wid, CancellationToken token = default)
    {
        try
        {
            // 原始 json：{"method":"getPersonalDataDetail","param":{"wid":"替换为实际值"}}
            var content = $$$"""{"method":"getPersonalDataDetail","param":{"wid":"{{{wid}}}"}}""";
            using var request = new HttpRequestMessage(HttpMethod.Post, string.Format(GSConst.EhallGetData, cardWid))
            {
                Content = new StringContent(content, Encoding.UTF8, new MediaTypeHeaderValue("application/json")),
            };
            using var response = await _client.SendAsync(cookies, request, false, token);
            var json = await response.Content.ReadAsStringAsync(token);
            _logger.LogCritical("{0}", json);
            return (decimal.Parse(json.Extract("余额：", "元", out var cur)), json.Extract("卡号：", '"', out _, cur));
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "获取校园卡信息", e);
            return null;
        }
    }

    public virtual async Task<(decimal Balance, int RemainingMonths)?> GetCampusNetworkInfo(CookieContainer cookies, string cardWid, string wid, CancellationToken token = default)
    {
        try
        {
            // 原始 json：{"method":"getPersonalDataDetail","param":{"wid":"替换为实际值"}}
            var content = $$$"""{"method":"getPersonalDataDetail","param":{"wid":"{{{wid}}}"}}""";
            using var request = new HttpRequestMessage(HttpMethod.Post, string.Format(GSConst.EhallGetData, cardWid))
            {
                Content = new StringContent(content, Encoding.UTF8, new MediaTypeHeaderValue("application/json")),
            };
            using var response = await _client.SendAsync(cookies, request, false, token);
            var json = await response.Content.ReadAsStringAsync(token);
            _logger.LogCritical("{0}", json);
            return (decimal.Parse(json.Extract("余额: ", "元", out var cur)), int.Parse(json.Extract("剩余月数: ", '个', out _, cur)));
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "获取校园网信息", e);
            return null;
        }
    }
}
