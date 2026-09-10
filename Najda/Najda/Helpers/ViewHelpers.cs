using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Najda.Helpers;

// Razor helper methods so views/partials can render shared UI without
// repeating markup:  @Html.Icon("logo")  @Html.Droplet("O+", true, "lg")
//                    @Html.StatusBadge("Urgent")  @Html.BtChip("O-")
public static class ViewHelpers
{
    // --- inline SVG icons (currentColor) ---
    private static readonly Dictionary<string, string> Icons = new()
    {
        ["logo"] = """<svg class="logo" viewBox="0 0 40 40" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true"><path d="M20 3C20 3 32 15.5 32 24.5C32 31.4 26.6 37 20 37C13.4 37 8 31.4 8 24.5C8 15.5 20 3 20 3Z" fill="#C0392B"/><path d="M20 12C20 12 26 18.5 26 23.5C26 27 23.3 30 20 30" stroke="#fff" stroke-width="2.4" stroke-linecap="round" fill="none" opacity=".85"/></svg>""",
        ["menu"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><path d="M4 7h16M4 12h16M4 17h16"/></svg>""",
        ["check"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M5 13l4 4L19 7"/></svg>""",
        ["pin"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 21s7-6.3 7-11a7 7 0 10-14 0c0 4.7 7 11 7 11z"/><circle cx="12" cy="10" r="2.5"/></svg>""",
        ["clock"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/></svg>""",
        ["drop"] = """<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 2s7 8 7 12.5A7 7 0 015 14.5C5 10 12 2 12 2z"/></svg>""",
        ["home"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 11l8-7 8 7"/><path d="M6 10v9h12v-9"/></svg>""",
        ["user"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.5-6 8-6s8 2 8 6"/></svg>""",
        ["list"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"/></svg>""",
        ["history"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M3 12a9 9 0 109-9 9 9 0 00-7 3.3M3 4v4h4"/><path d="M12 8v4l3 2"/></svg>""",
        ["plus"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><path d="M12 5v14M5 12h14"/></svg>""",
        ["building"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 21h16M6 21V5l6-2 6 2v16M10 9h.01M14 9h.01M10 13h.01M14 13h.01M10 17h.01M14 17h.01"/></svg>""",
        ["users"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="9" cy="8" r="3.2"/><path d="M3 20c0-3.3 2.7-5 6-5s6 1.7 6 5"/><path d="M16 6a3 3 0 010 6M21 20c0-2.6-1.4-4.2-3.5-4.8"/></svg>""",
        ["chart"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M4 20V10M10 20V4M16 20v-7M22 20H2"/></svg>""",
        ["logout"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 4h3a1 1 0 011 1v14a1 1 0 01-1 1h-3M10 17l5-5-5-5M15 12H3"/></svg>""",
        ["search"] = """<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="11" cy="11" r="7"/><path d="M21 21l-4-4"/></svg>""",
        ["heart"] = """<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 21s-7-4.6-9.5-9C.7 8.5 2.4 5 6 5c2.1 0 3.4 1.2 4 2 .6-.8 1.9-2 4-2 3.6 0 5.3 3.5 3.5 7-2.5 4.4-9.5 9-9.5 9z"/></svg>""",
    };

    public static IHtmlContent Icon(this IHtmlHelper _, string name)
        => new HtmlString(Icons.TryGetValue(name, out var svg) ? svg : "");

    // --- blood-type droplet token (dashed ring = unverified, gold ring + check = verified) ---
    public static IHtmlContent Droplet(this IHtmlHelper _, string bloodType, bool verified, string? size = null)
    {
        var cls = string.IsNullOrEmpty(size) ? "" : $" droplet-{size}";
        var state = verified ? "is-verified" : "is-unverified";
        var check = verified ? $"<span class=\"check\">{Icons["check"]}</span>" : "";
        return new HtmlString(
            $"<div class=\"droplet {state}{cls}\"><span class=\"shape\"></span><span class=\"type\">{bloodType}</span>{check}</div>");
    }

    // --- inline blood-type chip ---
    public static IHtmlContent BtChip(this IHtmlHelper _, string bloodType)
        => new HtmlString($"<span class=\"bt-chip\">{bloodType}</span>");

    // --- status badge ---
    private static readonly Dictionary<string, (string cls, string ar)> Badges = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Urgent"] = ("badge-urgent", "عاجل"),
        ["Open"] = ("badge-open", "مفتوح"),
        ["Matched"] = ("badge-matched", "تمت المطابقة"),
        ["Fulfilled"] = ("badge-fulfilled", "مكتمل"),
        ["Cancelled"] = ("badge-unverified", "ملغى"),
        ["Verified"] = ("badge-verified", "موثّقة"),
        ["Unverified"] = ("badge-unverified", "غير موثّقة"),
        ["Approved"] = ("badge-fulfilled", "معتمد"),
        ["Pending"] = ("badge-matched", "قيد المراجعة"),
        ["Suspended"] = ("badge-unverified", "معلّق"),
    };

    public static IHtmlContent StatusBadge(this IHtmlHelper _, string key)
    {
        if (!Badges.TryGetValue(key ?? "", out var b)) b = Badges["Open"];
        var dot = key is not null && key.Equals("Urgent", StringComparison.OrdinalIgnoreCase) ? Icons["drop"] : "";
        return new HtmlString(
            $"<span class=\"badge {b.cls}\">{dot}<span>{b.ar}</span></span>");
    }

    // Request badge: shows "Urgent" only while still open; otherwise the status.
    public static IHtmlContent RequestBadge(this IHtmlHelper html, string priority, string status)
    {
        var key = (priority == "Urgent" && status == "Open") ? "Urgent" : status;
        return html.StatusBadge(key);
    }
}
