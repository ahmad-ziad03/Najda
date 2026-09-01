/* ---------------- Language ---------------- */
const LANG_KEY = "najda-lang";

function currentLang() {
    return localStorage.getItem(LANG_KEY) || "ar";
}

function applyLang(lang) {
    const html = document.documentElement;
    html.setAttribute("lang", lang);
    html.setAttribute("dir", lang === "ar" ? "rtl" : "ltr");
    localStorage.setItem(LANG_KEY, lang);

    // input placeholders (can't use dual nodes)
    document.querySelectorAll("[data-ph-ar]").forEach((el) => {
        el.setAttribute("placeholder", el.getAttribute(lang === "ar" ? "data-ph-ar" : "data-ph-en") || "");
    });

    // toggle buttons show the OTHER language
    document.querySelectorAll("[data-lang-label]").forEach((el) => {
        el.textContent = lang === "ar" ? "EN" : "ع";
    });

    // <option> text (can't use dual nodes) via data-ar / data-en
    document.querySelectorAll("select option[data-ar]").forEach((o) => {
        o.textContent = o.getAttribute("data-" + lang);
    });

    document.dispatchEvent(new CustomEvent("langchange", { detail: { lang } }));
}

function toggleLang() {
    applyLang(currentLang() === "ar" ? "en" : "ar");
}

/* ---------------- Toast ---------------- */
const TOAST_IC = {
    success: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M5 13l4 4L19 7"/></svg>`,
    info: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"><path d="M12 8v5"/><path d="M12 16h.01"/></svg>`,
    error: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round"><path d="M6 6l12 12M18 6L6 18"/></svg>`,
};

function showToast(msg, type = "success") {
    let host = document.getElementById("toastHost");
    if (!host) {
        host = document.createElement("div");
        host.id = "toastHost";
        host.className = "toast-host";
        document.body.appendChild(host);
    }
    const t = document.createElement("div");
    t.className = "toast toast-" + type;
    t.innerHTML = `<span class="toast-ic">${TOAST_IC[type] || TOAST_IC.success}</span><span class="toast-msg">${msg}</span>`;
    host.appendChild(t);
    requestAnimationFrame(() => t.classList.add("show"));
    setTimeout(() => { t.classList.remove("show"); setTimeout(() => t.remove(), 260); }, 2600);
}

/* ---------------- Mobile dashboard sidebar ---------------- */
function openSide() {
    document.getElementById("side")?.classList.add("open");
    document.getElementById("scrim")?.classList.add("open");
}
function closeSide() {
    document.getElementById("side")?.classList.remove("open");
    document.getElementById("scrim")?.classList.remove("open");
}

/* ---------------- Boot ---------------- */
document.addEventListener("DOMContentLoaded", () => {
    applyLang(currentLang());

    // public nav hamburger
    const burger = document.querySelector("[data-nav-burger]");
    if (burger) burger.addEventListener("click", () => document.getElementById("navlinks")?.classList.toggle("open"));

    // server-side toast (set by a controller via TempData)
    const st = document.getElementById("serverToast");
    if (st) showToast(st.getAttribute("data-msg"), st.getAttribute("data-type") || "success");
});