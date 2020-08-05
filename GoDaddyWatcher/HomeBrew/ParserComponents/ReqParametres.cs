﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
 using GoDaddyWatcher.HomeBrew;
 using Homebrew.Enums;

namespace Homebrew.ParserComponents
{
    /// <summary>
    ///     Класс отвечает за создание и настройку нового запрос.
    ///     <para>
    ///         Не работает без инициализации конструктора
    ///         <seealso cref="ReqParametres" />
    ///     </para>
    /// </summary>
    public class ReqParametres
    {
        private readonly HttpMethod _httpMethod;
        private readonly string _reqData;

        /// <summary>
        ///     Основной метод создания нового запроса.
        ///     <para><seealso cref="SetUserAgent(string)" /> - установка Юзер-агента</para>
        ///     <para><see cref="SetProxy" /> - установка прокси</para>
        ///     <para>
        ///         <see cref="SetReqAdditionalParametres(bool,int,string)" /> - установка дополнительных
        ///         параметров
        ///     </para>
        /// </summary>
        /// <param name="link">Основная ссылка</param>
        /// <param name="httpMethod">Тип запроса (Get, Post,Put и т.д.)</param>
        /// <param name="reqData">Тело запроса</param>
        public ReqParametres(string link, HttpMethod httpMethod = HttpMethod.Get, string reqData = "")
        {
            RowRequest = (HttpWebRequest) WebRequest.Create(LinkFormatter(link));
            _httpMethod = httpMethod;
            _reqData = reqData;
        }

        public HttpWebRequest Request
        {
            get
            {
                RowRequest.Method = _httpMethod.ToString();
                if (_reqData.Length > 0)
                {
                    var byte1 = Encoding.UTF8.GetBytes(_reqData);
                    RowRequest.ContentLength = byte1.Length;
                    Stream newStream = RowRequest.GetRequestStream();
                    newStream.Write(byte1, 0, byte1.Length);
                }

                return RowRequest;
            }
        }

        public HttpWebRequest RowRequest { get; }

        /// <summary>
        ///     Установка дополнительных параметров
        /// </summary>
        /// <param name="allowAutoRedirect">Включить авторедирект?</param>
        /// <param name="maximumAutomaticRedirections">Максимальное количество редиректов</param>
        /// <param name="contentType">Тип контента задаётся через класс "ParserContentType"</param>
        /// <param name="cookieCollection">Cookie задаются через "CookieCollection"</param>
        public void SetReqAdditionalParametres(bool allowAutoRedirect = true, int maximumAutomaticRedirections = 100,
            string contentType = "application/x-www-form-urlencoded")
        {
            RowRequest.MaximumAutomaticRedirections = maximumAutomaticRedirections;
            RowRequest.AllowAutoRedirect = allowAutoRedirect;
            RowRequest.ContentType = contentType;
        }

        /// <summary>
        ///     Установка прокси
        /// </summary>
        /// <param name="proxy">IP-адрес прокси</param>
        /// <param name="timeOut"></param>
        public bool SetProxy(string proxy, int timeOut=5000)
        {
            SetTimout(timeOut);
            try
            {
                var tempProxy = proxy.StartsWith("http") ? proxy : $"http://{proxy}";
                RowRequest.Proxy = new WebProxy(new Uri(tempProxy))
                {
                    Credentials = new NetworkCredential(Proxies.Login, Proxies.Password)
                };
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Установка юзер-агента
        /// </summary>
        /// <param name="userAgent">Юзер-агент</param>
        public void SetUserAgent(string userAgent)
        {
            RowRequest.UserAgent = userAgent;
        }

        public void SetTimout(int timeout)
        {
            RowRequest.Timeout = timeout;
            RowRequest.ReadWriteTimeout = timeout;
        }

        public void SetCookie(CookieContainer cookieContainer)
        {
            RowRequest.CookieContainer = cookieContainer;
        }

        public void AddCookie(Cookie cookie)
        {
            if (RowRequest.CookieContainer == null) RowRequest.CookieContainer = new CookieContainer();
            RowRequest.CookieContainer.Add(cookie);
        }

        public void AddCookie(CookieCollection cookie)
        {
            if (RowRequest.CookieContainer == null) RowRequest.CookieContainer = new CookieContainer();
            RowRequest.CookieContainer.Add(cookie);
        }

        public void AddCookie(string cookie, bool multiline = false)
        {
            if (!multiline)
            {
                var name = "";
                var value = "";
                if (cookie.Contains("=") && cookie.Split('=').Length == 2)
                {
                    name = cookie.Split('=')[0];
                    value = cookie.Split('=')[1];
                    AddCookie(new Cookie(name, value));
                }
            }
            else
            {
                if (cookie.Contains("="))
                {
                    var allCookies = new List<string>(cookie.Split(';'));
                    var cookieCollection = new CookieCollection();
                    allCookies.ForEach(cook =>
                    {
                        if (cook.Contains("=") && cook.Split('=').Length == 2)
                            cookieCollection.Add(
                                new Cookie(
                                    cook.Split('=')[0],
                                    cook.Split('=')[1]
                                ));
                    });
                    AddCookie(cookieCollection);
                }
            }
        }

        private static string LinkFormatter(string link)
        {
            var newLink = link;
            if (!newLink.StartsWith("http"))
            {
                if (!newLink.Contains("www.")) newLink = $"www.{newLink}";
                newLink = $"http://{newLink}";
            }

            return newLink;
        }
    }
}