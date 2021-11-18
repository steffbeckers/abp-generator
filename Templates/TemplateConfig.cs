namespace SteffBeckers.Abp.Generator.Templates
{
    /// <summary>
    /// Add template config to top of .hbs template, e.g.:
    ///
    /// {
    ///  "RunForEachEntity": true
    /// }
    /// #-#-#
    /// using System;
    /// ...
    ///
    /// </summary>
    public class TemplateConfig
    {
        public bool RunForEachEntity { get; set; } = false;
    }
}