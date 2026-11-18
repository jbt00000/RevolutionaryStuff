using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RevolutionaryStuff.AspNetCore;

public static class PageAlertHelpers
{
    private const string PageAlertsKey = "PageAlerts";

    extension(ITempDataDictionary sources)
    {
        public void AddPageAlert(PageAlert pa)
        {
            if (pa == null || string.IsNullOrEmpty(pa.Message)) return;
            var pageAlerts = sources.GetPageAlerts(false);
            pageAlerts ??= [];
            pageAlerts.Add(pa);
            sources.SetPageAlerts(pageAlerts);
        }
        public IList<PageAlert> GetPageAlerts(bool clear)
        {
            var alerts = new List<PageAlert>();
            if (sources.Peek(PageAlertsKey) is string json && json != "")
            {
                var z = JsonHelpers.FromJson<IList<PageAlert>>(json);
                alerts.AddRange(z);
            }
            if (clear)
            {
                sources.Remove(PageAlertsKey);
            }
            return alerts;
        }
        public void SetPageAlerts(IList<PageAlert> alerts)
        {
            var json = JsonHelpers.ToJson(alerts ?? PageAlert.None);
            sources[PageAlertsKey] = json;
        }
    }




}
