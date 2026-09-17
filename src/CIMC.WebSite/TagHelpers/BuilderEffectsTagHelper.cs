using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;

namespace MySite.Web.TagHelpers
{
    /// <summary>Attach effect settings to the real component element without adding a layout wrapper.</summary>
    [HtmlTargetElement(Attributes = "sb-effects")]
    public class BuilderEffectsTagHelper : TagHelper
    {
        public IDictionary<string, object> SbEffects { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll("sb-effects");
            var keys = new[] { "effectEntrance", "effectHover", "effectDuration", "effectDelay", "effectDistance", "effectRepeat", "effectEasing", "effectCounter" };
            var settings = keys.Where(key => SbEffects != null && SbEffects.ContainsKey(key))
                .ToDictionary(key => key, key => SbEffects[key]);
            if (settings.Count > 0)
                output.Attributes.SetAttribute("data-sb-effects", JsonConvert.SerializeObject(settings));
        }
    }
}
