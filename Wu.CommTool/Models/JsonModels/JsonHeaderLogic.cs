﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Ioc;
using Prism.Regions;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace Wu.CommTool.Models.JsonModels
{
    public class JsonHeaderLogic : BindableBase
    {
        //用于界面绑定的属性定义
        /// <summary>
        /// Header
        /// </summary>
        public string Header { get => _Header; set => SetProperty(ref _Header, value); }
        private string _Header;
        public IEnumerable<JsonHeaderLogic> Children { get; private set; }

        public JToken Token { get; private set; }




        //内部构造函数，使用FromJToken来创建JsonHeaderLogic
        JsonHeaderLogic(JToken token, string header, IEnumerable<JsonHeaderLogic> children)
        {
            Token = token;
            Header = header;
            Children = children;
        }



        //外部的从JToken创建JsonHeaderLogic的方法
        public static JsonHeaderLogic FromJToken(JToken jtoken)
        {
            if (jtoken == null)
            {
                throw new ArgumentNullException("jtoken");
            }
            var type = jtoken.GetType();
            if (typeof(JValue).IsAssignableFrom(type))
            {
                var jvalue = (JValue)jtoken;
                var value = jvalue.Value;
                if (value == null)
                    value = "<null>";
                return new JsonHeaderLogic(jvalue, value.ToString(), null);
            }
            else if (typeof(JContainer).IsAssignableFrom(type))
            {
                var jcontainer = (JContainer)jtoken;
                var children = jcontainer.Children().Select(c => FromJToken(c));
                string header;

                if (typeof(JProperty).IsAssignableFrom(type))
                    header = ((JProperty)jcontainer).Name;                   
                //header = String.Format(PROPERTY, ((JProperty)jcontainer).Name);
                else if (typeof(JArray).IsAssignableFrom(type))
                    //header = String.Format(ARRAY, children.Count());
                    header = $"[ {children.Count()} ]";
                else if (typeof(JObject).IsAssignableFrom(type))
                    //header = String.Format(OBJECT, children.Count());
                    header = $"{{ {children.Count()} }}";
                else
                    throw new Exception("不支持的JContainer类型");
                return new JsonHeaderLogic(jcontainer, header, children);
            }
            else
            {
                throw new Exception("不支持的JToken类型");
            }
        }
    }
}