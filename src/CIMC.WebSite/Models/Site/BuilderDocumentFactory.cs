using System;
using System.Collections.Generic;
using System.Linq;

namespace MySite.Web.Models
{
    public static class BuilderDocumentFactory
    {
        public const string GlobalHeaderPageCode = "__GLOBAL_HEADER__";
        public const string GlobalFooterPageCode = "__GLOBAL_FOOTER__";
        public const string GlobalHeaderPath = "/__global/header";
        public const string GlobalFooterPath = "/__global/footer";

        public static BuilderDocumentModel CreateEmpty(string name)
        {
            return new BuilderDocumentModel { SchemaVersion = 1, Name = name ?? string.Empty };
        }

        public static BuilderDocumentModel CreateDefaultHeader()
        {
            return new BuilderDocumentModel
            {
                Name = "Header",
                Nodes = new List<BuilderNodeModel>
                {
                    Node("section", null, Style("paddingTop","16px","paddingBottom","16px","backgroundColor","#ffffff","position","sticky","top","0px","zIndex",1000,"boxShadow","0 2px 8px rgba(0,0,0,.06)"),
                        Node("container", null, null,
                            Node("grid", Props("columns",3), Style("gap","20px"),
                                Node("column", null, null, Node("logo", Props("text","企业名称","href","/"), null)),
                                Node("column", null, Style("textAlign","center"), Node("navigation", Props("direction","horizontal"), null)),
                                Node("column", null, Style("textAlign","right"), Node("button", Props("text","联系我们","href","/contact","variant","outline"), null)))))
                }
            };
        }

        public static BuilderDocumentModel CreateDefaultFooter()
        {
            return new BuilderDocumentModel
            {
                Name = "Footer",
                Nodes = new List<BuilderNodeModel>
                {
                    Node("section", null, Style("paddingTop","48px","paddingBottom","24px","backgroundColor","#111827","color","#ffffff"),
                        Node("container", null, null,
                            Node("grid", Props("columns",3), Style("gap","36px"),
                                Node("column", null, null, Node("logo", Props("text","企业名称","href","/"), null)),
                                Node("column", null, null, Node("navigation", Props("direction","vertical"), null)),
                                Node("column", null, null, Node("contact", new Dictionary<string, object>(), null))),
                            Node("divider", null, null),
                            Node("copyright", Props("text","© 2026 企业名称 版权所有"), Style("textAlign","center"))))
                }
            };
        }

        public static BuilderNodeModel Node(string type, Dictionary<string, object> props, Dictionary<string, object> style, params BuilderNodeModel[] children)
        {
            return new BuilderNodeModel
            {
                Id = type + "_" + Guid.NewGuid().ToString("N").Substring(0, 10),
                Type = type,
                Version = 1,
                Name = type,
                Props = props ?? new Dictionary<string, object>(),
                Style = style ?? new Dictionary<string, object>(),
                Children = children == null ? new List<BuilderNodeModel>() : children.ToList()
            };
        }

        private static Dictionary<string, object> Props(params object[] values)
        {
            return Dictionary(values);
        }

        private static Dictionary<string, object> Style(params object[] values)
        {
            return Dictionary(values);
        }

        private static Dictionary<string, object> Dictionary(params object[] values)
        {
            var result = new Dictionary<string, object>();
            for (var i = 0; i + 1 < values.Length; i += 2)
            {
                result[values[i].ToString()] = values[i + 1];
            }
            return result;
        }
    }
}
