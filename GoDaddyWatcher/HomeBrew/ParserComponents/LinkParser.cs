﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Homebrew.ParserComponents
{
    /// <summary>
    ///     Парсер ссылок.
    ///     Принимает HttpWebRequest в конструктор
    /// </summary>
    /// <seealso cref="ReqParametres" />
    public class LinkParser
    {
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly HttpWebRequest _request;

        public LinkParser(HttpWebRequest httpRequest)
        {
            _request = httpRequest;
            StartParsing();
        }
        public LinkParser(HttpWebRequest httpRequest, Encoding encoding)
        {
            _encoding = encoding;
            _request = httpRequest;
            StartParsing();
        }

        public string Data { get; private set; } = "";
        public CookieContainer Cookies { get; private set; }
        public bool IsError { get; private set; }
        
        private bool _hasAnswer;

        private void StartParsing()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Thread thread = new Thread(() =>
            {
                if (_request.CookieContainer == null || _request.CookieContainer.Count == 0)
                    _request.CookieContainer = new CookieContainer();
                try
                {
                    string data;
                    var response = (HttpWebResponse) _request.GetResponse();
                    IsError = response.StatusCode != HttpStatusCode.OK;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream receiveStream = response.GetResponseStream();
                        StreamReader readStream;
                        if (response.CharacterSet == null)
                            readStream = new StreamReader(receiveStream);
                        else
                            readStream = new StreamReader(receiveStream, _encoding);
                        data = readStream.ReadToEnd();
                        var cookieContainer = new CookieContainer();
                        foreach (Cookie cookie in response.Cookies) cookieContainer.Add(cookie);
                        Cookies = cookieContainer;
                        response.Close();
                        readStream.Close();
                    }
                    else
                    {
                        data = "";
                    }

                    Data = data;
                    _hasAnswer = true;
                }
                catch (Exception)
                {
                    IsError = true;
                    _hasAnswer = true;
                }
            }){IsBackground = true};
            thread.Start();
            while (_hasAnswer==false && stopwatch.ElapsedMilliseconds<_request.Timeout)
            {
                Thread.Sleep(100);
            }

            if (_hasAnswer == false)
            {
                IsError = true;
            }
            try
            {
                thread.Abort();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}