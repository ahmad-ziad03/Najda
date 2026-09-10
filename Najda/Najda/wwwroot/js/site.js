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

/* ---------------- Confirm dialog (replaces window.confirm) ----------------
   Usage on a form:  onsubmit="return najdaConfirm(this, 'العنوان', 'الرسالة')"
   (some call sites still pass extra English arguments; they are accepted
   and ignored so nothing else needs to change.)
   Returns false and, if the user confirms in the dialog, submits the form. */
function najdaConfirm(form, title, message) {
    const scrim = document.getElementById("confirmScrim");
    if (!scrim) return true; // dialog not on page -> allow normal submit

    const t = document.getElementById("confirmTitle");
    const m = document.getElementById("confirmMsg");
    const ok = document.getElementById("confirmOk");
    const cancel = document.getElementById("confirmCancel");

    if (t) t.textContent = title || "تأكيد الإجراء";
    if (m) m.textContent = message || "";

    scrim.classList.add("open");

    const close = () => {
        scrim.classList.remove("open");
        ok.onclick = null; cancel.onclick = null; scrim.onclick = null;
    };
    ok.onclick = () => { close(); form.submit(); };
    cancel.onclick = close;
    scrim.onclick = (e) => { if (e.target === scrim) close(); };

    return false; // block the immediate submit; we submit after confirmation
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
    // public nav hamburger
    const burger = document.querySelector("[data-nav-burger]");
    if (burger) burger.addEventListener("click", () => document.getElementById("navlinks")?.classList.toggle("open"));

    // server-side toast (set by a controller via TempData)
    const st = document.getElementById("serverToast");
    if (st) showToast(st.getAttribute("data-msg"), st.getAttribute("data-type") || "success");
});
