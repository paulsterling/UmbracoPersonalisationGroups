﻿namespace Zone.UmbracoPersonalisationGroups.Criteria.Cookie
{
    using System;
    using Helpers;
    using Newtonsoft.Json;
    using Umbraco.Core;

    /// <summary>
    /// Implements a personalisation group criteria based on the presence, absence or value of a cookie
    /// </summary>
    public class CookiePersonalisationGroupCriteria : IPersonalisationGroupCriteria
    {
        private readonly ICookieProvider _cookieProvider;

        public CookiePersonalisationGroupCriteria()
        {
            _cookieProvider = new HttpContextCookieProvider();
        }

        public CookiePersonalisationGroupCriteria(ICookieProvider cookieProvider)
        {
            _cookieProvider = cookieProvider;
        }

        public string Name
        {
            get { return "Cookie"; }
        }

        public string Alias
        {
            get { return "cookie"; }
        }

        public string Description
        {
            get { return "Matches visitor session with the presence, absence or value of a cookie"; }
        }

        public bool MatchesVisitor(string definition)
        {
            Mandate.ParameterNotNullOrEmpty(definition, "definition");

            CookieSetting cookieSetting;
            try
            {
                cookieSetting = JsonConvert.DeserializeObject<CookieSetting>(definition);
            }
            catch (JsonReaderException)
            {
                throw new ArgumentException(string.Format("Provided definition is not valid JSON: {0}", definition));
            }

            if (string.IsNullOrEmpty(cookieSetting.Key))
            {
                throw new ArgumentNullException("key", "Cookie key not set");
            }

            var cookieExists = _cookieProvider.CookieExists(cookieSetting.Key);
            var cookieValue = string.Empty;
            if (cookieExists)
            {
                cookieValue = _cookieProvider.GetCookieValue(cookieSetting.Key);
            }

            switch (cookieSetting.Match)
            {
                case CookieSettingMatch.Exists:
                    return cookieExists;
                case CookieSettingMatch.DoesNotExist:
                    return !cookieExists;
                case CookieSettingMatch.MatchesValue:
                    return cookieExists && cookieValue == cookieSetting.Value;
                case CookieSettingMatch.ContainsValue:
                    return cookieExists && cookieValue.Contains(cookieSetting.Value);
                case CookieSettingMatch.GreaterThanValue:
                case CookieSettingMatch.GreaterThanOrEqualToValue:
                case CookieSettingMatch.LessThanValue:
                case CookieSettingMatch.LessThanOrEqualToValue:
                    return cookieExists &&
                        ComparisonHelpers.CompareValues(cookieValue, cookieSetting.Value, GetComparison(cookieSetting.Match));
                default:
                    return false;
            }
        }

        private static Comparison GetComparison(CookieSettingMatch settingMatch)
        {
            switch (settingMatch)
            {
                case CookieSettingMatch.GreaterThanValue:
                    return Comparison.GreaterThan;
                case CookieSettingMatch.GreaterThanOrEqualToValue:
                    return Comparison.GreaterThanOrEqual;
                case CookieSettingMatch.LessThanValue:
                    return Comparison.LessThan;
                case CookieSettingMatch.LessThanOrEqualToValue:
                    return Comparison.LessThanOrEqual;
                default:
                    throw new ArgumentException("Setting supplied does not match a comparison type", "settingMatch");
            }
        }
    }
}
