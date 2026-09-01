/* =====================================================================
   NAJDA — shared application script
   - Language toggle (AR default / EN), persisted in localStorage
   - Injects shared chrome: public nav, footer, dashboard sidebar
   - Holds all demo (fake) data used across pages
   Bilingual text uses dual nodes: <x data-lang-ar> / <x data-lang-en>.
   CSS shows only the active language once <html lang> is set.
   ===================================================================== */

/* ------------------------------------------------------------------ *
 * 1. LANGUAGE
 * ------------------------------------------------------------------ */
const LANG_KEY = "najda-lang";

function currentLang() {
    return localStorage.getItem(LANG_KEY) || "ar";
}

function applyLang(lang) {
    const html = document.documentElement;
    html.setAttribute("lang", lang);
    html.setAttribute("dir", lang === "ar" ? "rtl" : "ltr");
    localStorage.setItem(LANG_KEY, lang);

    // placeholders (inputs can't use dual nodes)
    document.querySelectorAll("[data-ph-ar]").forEach((el) => {
        el.setAttribute(
            "placeholder",
            el.getAttribute(lang === "ar" ? "data-ph-ar" : "data-ph-en") || "",
        );
    });

    // update every language-toggle button label to show the OTHER language
    document.querySelectorAll("[data-lang-label]").forEach((el) => {
        el.textContent = lang === "ar" ? "EN" : "ع";
    });

    // let pages react (e.g. re-render dynamic lists) if they want
    document.dispatchEvent(new CustomEvent("langchange", { detail: { lang } }));
}

function toggleLang() {
    applyLang(currentLang() === "ar" ? "en" : "ar");
}

/* ------------------------------------------------------------------ *
 * 2. DEMO DATA  (stand-in for the SQL Server back end)
 * ------------------------------------------------------------------ */
const DATA = {
    bloodTypes: ["O+", "O-", "A+", "A-", "B+", "B-", "AB+", "AB-"],

    cities: [
        { ar: "عمّان", en: "Amman" },
        { ar: "إربد", en: "Irbid" },
        { ar: "الزرقاء", en: "Zarqa" },
        { ar: "العقبة", en: "Aqaba" },
        { ar: "المفرق", en: "Mafraq" },
        { ar: "الكرك", en: "Karak" },
    ],

    // The signed-in donor (used across donor pages)
    me: {
        name: { ar: "سامر الخطيب", en: "Samer Al-Khatib" },
        initials: "س",
        bloodType: "O+",
        verified: true,
        city: { ar: "عمّان", en: "Amman" },
        area: { ar: "الجبيهة", en: "Al-Jubaiha" },
        donations: 4,
        lastDonation: "2025-07-02",
        nextEligible: "2025-08-27",
        available: true,
    },

    donors: [
        {
            id: 1,
            name: { ar: "سامر الخطيب", en: "Samer Al-Khatib" },
            bt: "O+",
            city: { ar: "عمّان", en: "Amman" },
            verified: true,
            donations: 4,
            available: true,
        },
        {
            id: 2,
            name: { ar: "ليلى منصور", en: "Layla Mansour" },
            bt: "A-",
            city: { ar: "إربد", en: "Irbid" },
            verified: true,
            donations: 7,
            available: true,
        },
        {
            id: 3,
            name: { ar: "خالد الرشيد", en: "Khaled Al-Rashid" },
            bt: "B+",
            city: { ar: "الزرقاء", en: "Zarqa" },
            verified: false,
            donations: 0,
            available: true,
        },
        {
            id: 4,
            name: { ar: "رنا عبدالله", en: "Rana Abdullah" },
            bt: "O-",
            city: { ar: "عمّان", en: "Amman" },
            verified: true,
            donations: 12,
            available: false,
        },
        {
            id: 5,
            name: { ar: "يوسف حدّاد", en: "Yousef Haddad" },
            bt: "AB+",
            city: { ar: "العقبة", en: "Aqaba" },
            verified: false,
            donations: 1,
            available: true,
        },
        {
            id: 6,
            name: { ar: "مريم القيسي", en: "Mariam Al-Qaisi" },
            bt: "A+",
            city: { ar: "عمّان", en: "Amman" },
            verified: true,
            donations: 3,
            available: true,
        },
        {
            id: 7,
            name: { ar: "عمر السالم", en: "Omar Al-Salem" },
            bt: "O+",
            city: { ar: "المفرق", en: "Mafraq" },
            verified: true,
            donations: 5,
            available: true,
        },
        {
            id: 8,
            name: { ar: "هبة نصر", en: "Hiba Nasr" },
            bt: "B-",
            city: { ar: "الكرك", en: "Karak" },
            verified: false,
            donations: 0,
            available: false,
        },
    ],

    hospitals: [
        {
            id: 1,
            name: { ar: "مستشفى البشير", en: "Al-Bashir Hospital" },
            city: { ar: "عمّان", en: "Amman" },
            status: "approved",
            requests: 14,
        },
        {
            id: 2,
            name: { ar: "مستشفى الملك المؤسس", en: "King Abdullah Hospital" },
            city: { ar: "إربد", en: "Irbid" },
            status: "approved",
            requests: 9,
        },
        {
            id: 3,
            name: { ar: "مستشفى الأمير حمزة", en: "Prince Hamzah Hospital" },
            city: { ar: "عمّان", en: "Amman" },
            status: "approved",
            requests: 6,
        },
        {
            id: 4,
            name: { ar: "بنك الدم الوطني", en: "National Blood Bank" },
            city: { ar: "عمّان", en: "Amman" },
            status: "pending",
            requests: 0,
        },
    ],

    // status: urgent | open | matched | fulfilled
    requests: [
        {
            id: "R-1042",
            hospital: { ar: "مستشفى البشير", en: "Al-Bashir Hospital" },
            city: { ar: "عمّان", en: "Amman" },
            bt: "O-",
            units: 3,
            status: "urgent",
            posted: "2025-08-27T08:20",
            matched: 2,
            note: {
                ar: "حالة طوارئ — قسم العناية",
                en: "Emergency — ICU case",
            },
        },
        {
            id: "R-1041",
            hospital: {
                ar: "مستشفى الأمير حمزة",
                en: "Prince Hamzah Hospital",
            },
            city: { ar: "عمّان", en: "Amman" },
            bt: "O+",
            units: 2,
            status: "urgent",
            posted: "2025-08-27T07:05",
            matched: 5,
            note: { ar: "عملية جراحية عاجلة", en: "Urgent surgery" },
        },
        {
            id: "R-1039",
            hospital: {
                ar: "مستشفى الملك المؤسس",
                en: "King Abdullah Hospital",
            },
            city: { ar: "إربد", en: "Irbid" },
            bt: "A+",
            units: 4,
            status: "matched",
            posted: "2025-08-26T16:40",
            matched: 4,
            note: { ar: "مريض تلاسيميا", en: "Thalassemia patient" },
        },
        {
            id: "R-1037",
            hospital: { ar: "مستشفى البشير", en: "Al-Bashir Hospital" },
            city: { ar: "عمّان", en: "Amman" },
            bt: "B+",
            units: 1,
            status: "open",
            posted: "2025-08-26T11:15",
            matched: 1,
            note: { ar: "احتياط بنك الدم", en: "Blood bank reserve" },
        },
        {
            id: "R-1034",
            hospital: {
                ar: "مستشفى الملك المؤسس",
                en: "King Abdullah Hospital",
            },
            city: { ar: "إربد", en: "Irbid" },
            bt: "AB-",
            units: 2,
            status: "open",
            posted: "2025-08-25T09:30",
            matched: 0,
            note: { ar: "فصيلة نادرة", en: "Rare type" },
        },
        {
            id: "R-1030",
            hospital: {
                ar: "مستشفى الأمير حمزة",
                en: "Prince Hamzah Hospital",
            },
            city: { ar: "عمّان", en: "Amman" },
            bt: "O+",
            units: 3,
            status: "fulfilled",
            posted: "2025-08-23T14:00",
            matched: 3,
            note: { ar: "اكتملت بنجاح", en: "Completed successfully" },
        },
    ],

    // donation history for the signed-in donor
    history: [
        {
            id: "D-318",
            date: "2025-07-02",
            hospital: { ar: "مستشفى البشير", en: "Al-Bashir Hospital" },
            bt: "O+",
            status: "verified",
        },
        {
            id: "D-241",
            date: "2025-04-28",
            hospital: { ar: "بنك الدم الوطني", en: "National Blood Bank" },
            bt: "O+",
            status: "verified",
        },
        {
            id: "D-155",
            date: "2025-01-19",
            hospital: {
                ar: "مستشفى الأمير حمزة",
                en: "Prince Hamzah Hospital",
            },
            bt: "O+",
            status: "verified",
        },
        {
            id: "D-071",
            date: "2024-10-05",
            hospital: { ar: "مستشفى البشير", en: "Al-Bashir Hospital" },
            bt: "O+",
            status: "verified",
        },
    ],
};

/* pick localized value {ar,en} for current language */
function L(obj) {
    if (obj == null) return "";
    if (typeof obj === "string") return obj;
    return obj[currentLang()] ?? obj.ar ?? "";
}

/* ------------------------------------------------------------------ *
 * 3. SHARED ICONS  (inline SVG, currentColor)
 * ------------------------------------------------------------------ */
const IC = {
    logo: `<svg class="logo" viewBox="0 0 40 40" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
    <path d="M20 3C20 3 32 15.5 32 24.5C32 31.4 26.6 37 20 37C13.4 37 8 31.4 8 24.5C8 15.5 20 3 20 3Z" fill="#C0392B"/>
    <path d="M20 12C20 12 26 18.5 26 23.5C26 27 23.3 30 20 30" stroke="#fff" stroke-width="2.4" stroke-linecap="round" fill="none" opacity=".85"/>
  </svg>`,
    globe: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3c2.5 2.7 2.5 15.3 0 18M12 3c-2.5 2.7-2.5 15.3 0 18"/></svg>`,
    menu: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><path d="M4 7h16M4 12h16M4 17h16"/></svg>`,
    check: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M5 13l4 4L19 7"/></svg>`,
    pin: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 21s7-6.3 7-11a7 7 0 10-14 0c0 4.7 7 11 7 11z"/><circle cx="12" cy="10" r="2.5"/></svg>`,
    clock: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/></svg>`,
    drop: `<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 2s7 8 7 12.5A7 7 0 015 14.5C5 10 12 2 12 2z"/></svg>`,
    home: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 11l8-7 8 7"/><path d="M6 10v9h12v-9"/></svg>`,
    user: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.5-6 8-6s8 2 8 6"/></svg>`,
    bell: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M6 9a6 6 0 1112 0c0 5 2 6 2 6H4s2-1 2-6z"/><path d="M10 20a2 2 0 004 0"/></svg>`,
    list: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"/></svg>`,
    history: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M3 12a9 9 0 109-9 9 9 0 00-7 3.3M3 4v4h4"/><path d="M12 8v4l3 2"/></svg>`,
    plus: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round"><path d="M12 5v14M5 12h14"/></svg>`,
    building: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 21h16M6 21V5l6-2 6 2v16M10 9h.01M14 9h.01M10 13h.01M14 13h.01M10 17h.01M14 17h.01"/></svg>`,
    users: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="9" cy="8" r="3.2"/><path d="M3 20c0-3.3 2.7-5 6-5s6 1.7 6 5"/><path d="M16 6a3 3 0 010 6M21 20c0-2.6-1.4-4.2-3.5-4.8"/></svg>`,
    chart: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M4 20V10M10 20V4M16 20v-7M22 20H2"/></svg>`,
    logout: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 4h3a1 1 0 011 1v14a1 1 0 01-1 1h-3M10 17l5-5-5-5M15 12H3"/></svg>`,
    search: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><circle cx="11" cy="11" r="7"/><path d="M21 21l-4-4"/></svg>`,
    heart: `<svg viewBox="0 0 24 24" fill="currentColor"><path d="M12 21s-7-4.6-9.5-9C.7 8.5 2.4 5 6 5c2.1 0 3.4 1.2 4 2 .6-.8 1.9-2 4-2 3.6 0 5.3 3.5 3.5 7-2.5 4.4-9.5 9-9.5 9z"/></svg>`,
};

/* droplet token markup */
function droplet(bt, verified, size) {
    const cls = size ? " droplet-" + size : "";
    const state = verified ? "is-verified" : "is-unverified";
    const check = verified ? `<span class="check">${IC.check}</span>` : "";
    return `<div class="droplet ${state}${cls}"><span class="shape"></span><span class="type">${bt}</span>${check}</div>`;
}

/* status badge markup (bilingual) */
function statusBadge(status) {
    const map = {
        urgent: { cls: "badge-urgent", ar: "عاجل", en: "Urgent" },
        open: { cls: "badge-open", ar: "مفتوح", en: "Open" },
        matched: { cls: "badge-matched", ar: "تمت المطابقة", en: "Matched" },
        fulfilled: { cls: "badge-fulfilled", ar: "مكتمل", en: "Fulfilled" },
        verified: { cls: "badge-verified", ar: "موثّقة", en: "Verified" },
        unverified: {
            cls: "badge-unverified",
            ar: "غير موثّقة",
            en: "Unverified",
        },
        approved: { cls: "badge-fulfilled", ar: "معتمد", en: "Approved" },
        pending: { cls: "badge-matched", ar: "قيد المراجعة", en: "Pending" },
    };
    const s = map[status] || map.open;
    const dot = status === "urgent" ? IC.drop : "";
    return `<span class="badge ${s.cls}">${dot}<span data-lang-ar>${s.ar}</span><span data-lang-en>${s.en}</span></span>`;
}

/* relative-ish time label from ISO (kept simple/bilingual) */
function fmtDate(iso) {
    const d = new Date(iso);
    const day = String(d.getDate()).padStart(2, "0");
    const mon = String(d.getMonth() + 1).padStart(2, "0");
    const y = d.getFullYear();
    return `${day}/${mon}/${y}`;
}

/* ------------------------------------------------------------------ *
 * 4. SHARED CHROME (injected)
 * ------------------------------------------------------------------ */
function publicNav(active) {
    const link = (href, key, ar, en) =>
        `<a href="${href}" class="${active === key ? "active" : ""}"><span data-lang-ar>${ar}</span><span data-lang-en>${en}</span></a>`;
    return `
  <nav class="nav">
    <div class="wrap nav-inner">
      <a href="index.html" class="brand">${IC.logo}<span>نجدة<small data-lang-ar>منصة التبرع بالدم</small><small data-lang-en>Blood Donation Platform</small></span></a>
      <button class="nav-burger" aria-label="Menu" onclick="document.getElementById('navlinks').classList.toggle('open')">${IC.menu}</button>
      <div class="nav-links" id="navlinks">
        ${link("index.html", "home", "الرئيسية", "Home")}
        ${link("about.html", "about", "كيف تعمل", "How it works")}
        ${link("donor-requests.html", "requests", "الطلبات العاجلة", "Urgent requests")}
        <div class="nav-actions">
          <button class="lang-toggle" onclick="toggleLang()">${IC.globe}<span data-lang-label>EN</span></button>
          <a href="login.html" class="btn btn-ghost btn-sm"><span data-lang-ar>تسجيل الدخول</span><span data-lang-en>Log in</span></a>
          <a href="register.html" class="btn btn-primary btn-sm"><span data-lang-ar>سجّل الآن</span><span data-lang-en>Sign up</span></a>
        </div>
      </div>
    </div>
  </nav>`;
}

function siteFooter() {
    const col = (h_ar, h_en, links) => `
    <div><h4><span data-lang-ar>${h_ar}</span><span data-lang-en>${h_en}</span></h4>
    ${links.map(([href, ar, en]) => `<a href="${href}"><span data-lang-ar>${ar}</span><span data-lang-en>${en}</span></a>`).join("")}</div>`;
    return `
  <footer class="footer">
    <div class="wrap">
      <div class="footer-grid">
        <div>
          <a href="index.html" class="brand">${IC.logo}<span>نجدة</span></a>
          <p class="muted"><span data-lang-ar>منصة تربط متبرعي الدم بالحالات العاجلة خلال دقائق. مجانية للمتبرعين دائماً.</span><span data-lang-en>Connecting blood donors with urgent cases in minutes. Always free for donors.</span></p>
        </div>
        ${col("المنصة", "Platform", [
            ["about.html", "كيف تعمل", "How it works"],
            ["donor-requests.html", "الطلبات العاجلة", "Urgent requests"],
            ["register.html", "سجّل كمتبرع", "Become a donor"],
        ])}
        ${col("للمستشفيات", "For hospitals", [
            ["register.html", "انضم كمستشفى", "Join as hospital"],
            ["hospital-dashboard.html", "لوحة التحكم", "Dashboard"],
            ["about.html#pricing", "الشركاء والرعاة", "Partners & sponsors"],
        ])}
        ${col("الدعم", "Support", [
            ["#", "تواصل معنا", "Contact"],
            ["#", "الأسئلة الشائعة", "FAQ"],
            ["#", "الخصوصية", "Privacy"],
        ])}
      </div>
      <div class="footer-bottom">
        <span>© 2025 نجدة — Najda</span>
        <span data-lang-ar>مشروع تخرّج · Orange Coding Academy</span>
        <span data-lang-en>Capstone project · Orange Coding Academy</span>
      </div>
    </div>
  </footer>`;
}

/* dashboard sidebar per role: 'donor' | 'hospital' | 'admin' */
function sidebar(role, active) {
    const item = (href, key, icon, ar, en) =>
        `<a href="${href}" class="${active === key ? "active" : ""}">${icon}<span><span data-lang-ar>${ar}</span><span data-lang-en>${en}</span></span></a>`;

    const menus = {
        donor: [
            ["donor-dashboard.html", "dash", IC.home, "لوحتي", "Dashboard"],
            [
                "donor-requests.html",
                "requests",
                IC.list,
                "الطلبات المطابقة",
                "Matched requests",
            ],
            [
                "donor-history.html",
                "history",
                IC.history,
                "سجل التبرعات",
                "Donation history",
            ],
            [
                "donor-profile.html",
                "profile",
                IC.user,
                "ملفي الشخصي",
                "My profile",
            ],
        ],
        hospital: [
            [
                "hospital-dashboard.html",
                "dash",
                IC.home,
                "لوحة المستشفى",
                "Dashboard",
            ],
            [
                "hospital-post-request.html",
                "post",
                IC.plus,
                "طلب دم جديد",
                "New request",
            ],
            [
                "hospital-requests.html",
                "requests",
                IC.list,
                "إدارة الطلبات",
                "Manage requests",
            ],
            [
                "hospital-matched-donors.html",
                "matched",
                IC.users,
                "المتبرعون المطابقون",
                "Matched donors",
            ],
        ],
        admin: [
            ["admin-dashboard.html", "dash", IC.chart, "نظرة عامة", "Overview"],
            ["admin-users.html", "users", IC.users, "المتبرعون", "Donors"],
            [
                "admin-hospitals.html",
                "hospitals",
                IC.building,
                "المستشفيات",
                "Hospitals",
            ],
        ],
    };

    const users = {
        donor: {
            nm: { ar: "سامر الخطيب", en: "Samer Al-Khatib" },
            rl: { ar: "متبرع", en: "Donor" },
            in: "س",
        },
        hospital: {
            nm: { ar: "مستشفى البشير", en: "Al-Bashir Hospital" },
            rl: { ar: "مستشفى", en: "Hospital" },
            in: "ب",
        },
        admin: {
            nm: { ar: "إدارة نجدة", en: "Najda Admin" },
            rl: { ar: "مدير النظام", en: "Administrator" },
            in: "A",
        },
    };
    const roleLabel = {
        donor: ["حساب المتبرع", "Donor account"],
        hospital: ["حساب المستشفى", "Hospital account"],
        admin: ["لوحة الإدارة", "Admin panel"],
    };
    const u = users[role];

    return `
  <aside class="side" id="side">
    <a href="index.html" class="brand">${IC.logo}<span>نجدة<small data-lang-ar>${roleLabel[role][0]}</small><small data-lang-en>${roleLabel[role][1]}</small></span></a>
    <div class="side-label"><span data-lang-ar>القائمة</span><span data-lang-en>Menu</span></div>
    ${menus[role].map((m) => item(...m)).join("")}
    <div class="spacer"></div>
    <div class="side-user">
      <div class="av">${u.in}</div>
      <div><div class="nm"><span data-lang-ar>${u.nm.ar}</span><span data-lang-en>${u.nm.en}</span></div>
      <div class="rl"><span data-lang-ar>${u.rl.ar}</span><span data-lang-en>${u.rl.en}</span></div></div>
    </div>
    <a href="login.html">${IC.logout}<span><span data-lang-ar>تسجيل الخروج</span><span data-lang-en>Log out</span></span></a>
  </aside>
  <div class="scrim" id="scrim" onclick="closeSide()"></div>`;
}

/* dashboard topbar */
function topbar(titleAr, titleEn) {
    return `
  <header class="topbar">
    <button class="side-toggle" aria-label="Menu" onclick="openSide()">${IC.menu}</button>
    <h1><span data-lang-ar>${titleAr}</span><span data-lang-en>${titleEn}</span></h1>
    <div class="topbar-actions">
      <button class="lang-toggle" onclick="toggleLang()">${IC.globe}<span data-lang-label>EN</span></button>
      <a href="login.html" class="btn btn-ghost btn-sm">${IC.logout}<span data-lang-ar>خروج</span><span data-lang-en>Log out</span></a>
    </div>
  </header>`;
}

function openSide() {
    document.getElementById("side")?.classList.add("open");
    document.getElementById("scrim")?.classList.add("open");
}
function closeSide() {
    document.getElementById("side")?.classList.remove("open");
    document.getElementById("scrim")?.classList.remove("open");
}

/* ------------------------------------------------------------------ *
 * 5. TOAST — shared feedback message
 *    showToast("Saved")            -> success (default)
 *    showToast({ar:"تم", en:"Done"}, "info")
 *    types: success | info | error
 * ------------------------------------------------------------------ */
const TOAST_IC = {
    success: IC.check,
    info: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"><path d="M12 8v5"/><path d="M12 16h.01"/></svg>`,
    error: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round"><path d="M6 6l12 12M18 6L6 18"/></svg>`,
};
function showToast(msg, type = "success") {
    const text =
        msg && typeof msg === "object"
            ? (msg[currentLang()] ?? msg.ar ?? "")
            : msg;
    let host = document.getElementById("toastHost");
    if (!host) {
        host = document.createElement("div");
        host.id = "toastHost";
        host.className = "toast-host";
        document.body.appendChild(host);
    }
    const t = document.createElement("div");
    t.className = "toast toast-" + type;
    t.innerHTML = `<span class="toast-ic">${TOAST_IC[type] || TOAST_IC.success}</span><span class="toast-msg">${text}</span>`;
    host.appendChild(t);
    requestAnimationFrame(() => t.classList.add("show"));
    setTimeout(() => {
        t.classList.remove("show");
        setTimeout(() => t.remove(), 260);
    }, 2400);
}

/* ------------------------------------------------------------------ *
 * 6. BOOTSTRAP: inject chrome, then apply language
 * ------------------------------------------------------------------ */
function injectChrome() {
    const nav = document.querySelector("[data-nav]");
    if (nav) nav.outerHTML = publicNav(nav.getAttribute("data-nav"));

    const foot = document.querySelector("[data-footer]");
    if (foot) foot.outerHTML = siteFooter();

    const side = document.querySelector("[data-side]");
    if (side)
        side.outerHTML = sidebar(
            side.getAttribute("data-side"),
            side.getAttribute("data-active"),
        );

    const top = document.querySelector("[data-topbar]");
    if (top)
        top.outerHTML = topbar(
            top.getAttribute("data-title-ar"),
            top.getAttribute("data-title-en"),
        );
}

document.addEventListener("DOMContentLoaded", () => {
    injectChrome();
    if (typeof pageRender === "function") pageRender(); // page-specific dynamic content
    applyLang(currentLang());
});
