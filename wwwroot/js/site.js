// ============================================================
// Tella Store — site.js v2
// يجمع: CSRF • Cart • Wishlist • Notifications • Toast
//        Gallery • Recently Viewed • Theme Picker (محسّن)
//        In-View Animations • Search Bar Toggle
// ============================================================

/* ============================================================
   THEME PICKER — يُطبَّق فوراً قبل DOMContentLoaded لتفادي الوميض
   ============================================================ */

const BG_THEMES = [
  // === فاتحة ===
  { name:'ثلجي ناصع',      bg:'#F8F9FA', bgSec:'#F0F2F5', card:'#FFFFFF', fg:'#1A2540', muted:'#5A6A85', border:'#E2E8F0' },
  { name:'أبيض نقي',       bg:'#FFFFFF', bgSec:'#F5F5F5', card:'#FFFFFF', fg:'#111111', muted:'#888888', border:'#E5E5E5' },
  { name:'كتان دافئ',      bg:'#F5F0EB', bgSec:'#EFE7DD', card:'#FFFFFF', fg:'#1A1A1A', muted:'#6B6460', border:'#E7DED3' },
  { name:'رمال ناعمة',     bg:'#F7F2EC', bgSec:'#EDE5D8', card:'#FFFFFF', fg:'#1A1A1A', muted:'#6B6460', border:'#E0D5C7' },
  { name:'أوف وايت',       bg:'#FDFAF6', bgSec:'#F5EFE6', card:'#FFFFFF', fg:'#2D2416', muted:'#8B7355', border:'#E8DCC8' },
  { name:'وردي ناعم',      bg:'#FFF0F3', bgSec:'#FFE4E9', card:'#FFFFFF', fg:'#3D0017', muted:'#9B4F6A', border:'#FFB3C6' },
  { name:'أزرق سماوي',     bg:'#EFF6FF', bgSec:'#DBEAFE', card:'#FFFFFF', fg:'#1E3A5F', muted:'#4A7FAD', border:'#BFDBFE' },
  { name:'أخضر نعناعي',    bg:'#F0FDF4', bgSec:'#DCFCE7', card:'#FFFFFF', fg:'#14532D', muted:'#4D7C67', border:'#BBF7D0' },
  { name:'رمادي فاتح',     bg:'#F2F2F2', bgSec:'#E8E8E8', card:'#FFFFFF', fg:'#1A1A1A', muted:'#777777', border:'#D5D5D5' },
  { name:'لافندر',         bg:'#F5F3FF', bgSec:'#EDE9FE', card:'#FFFFFF', fg:'#2E1065', muted:'#7C6FA6', border:'#DDD6FE' },
  { name:'شمسي دافئ',     bg:'#FFFBEB', bgSec:'#FEF3C7', card:'#FFFFFF', fg:'#451A03', muted:'#92400E', border:'#FDE68A' },
  // === داكنة ===
  { name:'أونيكس',         bg:'#1A1A1A', bgSec:'#242424', card:'#2E2E2E', fg:'#F5F5F5', muted:'#A0A0A0', border:'#3A3A3A' },
  { name:'منتصف الليل',   bg:'#0D1B2A', bgSec:'#162232', card:'#1E2D3D', fg:'#E8F1F8', muted:'#7B9BB5', border:'#2A3F55' },
  { name:'الغابة العميقة', bg:'#0F2017', bgSec:'#172B1F', card:'#1F3829', fg:'#E8F5EE', muted:'#6A9E7F', border:'#2A4235' },
  { name:'بنفسجي داكن',   bg:'#1A0A2E', bgSec:'#24103E', card:'#2E1A4A', fg:'#F0EAFF', muted:'#9B85C8', border:'#3D2560' },
  { name:'فحمي',           bg:'#18181B', bgSec:'#27272A', card:'#3F3F46', fg:'#FAFAFA', muted:'#A1A1AA', border:'#52525B' },
];

const ACCENT_COLORS = [
  { name:'نيلي عميق',      hex:'#2E5EA8', light:'#4A80D0', rgb:'46,94,168'   },
  { name:'أسود',           hex:'#1A1A1A', light:'#3A3A3A', rgb:'26,26,26'    },
  { name:'وردي',           hex:'#E11D48', light:'#FB7185', rgb:'225,29,72'   },
  { name:'أزرق',           hex:'#0284C7', light:'#7DD3FC', rgb:'2,132,199'   },
  { name:'أخضر',           hex:'#059669', light:'#6EE7B7', rgb:'5,150,105'   },
  { name:'بنفسجي',         hex:'#7C3AED', light:'#C4B5FD', rgb:'124,58,237'  },
  { name:'برتقالي',        hex:'#EA580C', light:'#FDBA74', rgb:'234,88,12'   },
  { name:'ذهبي',           hex:'#B8860B', light:'#F0D77B', rgb:'184,134,11'  },
  { name:'فيروزي',         hex:'#0D9488', light:'#5EEAD4', rgb:'13,148,136'  },
  { name:'قرمزي',          hex:'#DC2626', light:'#FCA5A5', rgb:'220,38,38'   },
  { name:'وردي فاتح',      hex:'#DB2777', light:'#F9A8D4', rgb:'219,39,119'  },
  { name:'رمادي أردوازي',  hex:'#475569', light:'#94A3B8', rgb:'71,85,105'   },
];

const LS_BG     = 'tella-bg-theme';
const LS_ACCENT = 'tella-accent';

let currentBg     = parseInt(localStorage.getItem(LS_BG)     || '0');
let currentAccent = parseInt(localStorage.getItem(LS_ACCENT) || '0');

/** تطبيق الثيم — يُستدعى فوراً ثم عند كل تغيير */
function applyTheme() {
  const bg  = BG_THEMES[currentBg]        || BG_THEMES[0];
  const acc = ACCENT_COLORS[currentAccent] || ACCENT_COLORS[0];
  const r   = document.documentElement;
  r.style.setProperty('--background',           bg.bg);
  r.style.setProperty('--background-secondary', bg.bgSec);
  r.style.setProperty('--card-background',      bg.card);
  r.style.setProperty('--foreground',           bg.fg);
  r.style.setProperty('--muted-foreground',     bg.muted);
  r.style.setProperty('--border-color',         bg.border);
  r.style.setProperty('--theme-accent',         acc.hex);
  r.style.setProperty('--theme-accent-light',   acc.light);
  r.style.setProperty('--theme-accent-rgb',     acc.rgb);
}

// تطبيق فوري لتجنب وميض الألوان
applyTheme();

/** بناء محتوى modal الثيم */
function buildThemeModal(tab) {
  const content = document.getElementById('themeContent');
  if (!content) return;

  const lightBgs = BG_THEMES.slice(0, 11);
  const darkBgs  = BG_THEMES.slice(11);

  if (tab === 'bg') {
    content.innerHTML = `
      <div class="theme-group-title">الخلفيات الفاتحة</div>
      <div class="bg-swatches">
        ${lightBgs.map((t, i) => `
          <div class="bg-swatch ${i === currentBg ? 'active' : ''}" onclick="selectBg(${i})">
            <div class="bg-swatch-color" style="background:${t.bg};border:1px solid ${t.border}"></div>
            <div class="bg-swatch-label">${t.name}</div>
          </div>`).join('')}
      </div>
      <div class="theme-group-title">الخلفيات الداكنة</div>
      <div class="bg-swatches">
        ${darkBgs.map((t, i) => `
          <div class="bg-swatch ${(i + 11) === currentBg ? 'active' : ''}" onclick="selectBg(${i + 11})">
            <div class="bg-swatch-color" style="background:${t.bg};border:1px solid ${t.border}"></div>
            <div class="bg-swatch-label" style="color:#999">${t.name}</div>
          </div>`).join('')}
      </div>`;
  } else {
    content.innerHTML = `
      <div class="theme-group-title">ألوان التمييز</div>
      <div class="accent-swatches">
        ${ACCENT_COLORS.map((a, i) => `
          <div>
            <div class="accent-swatch ${i === currentAccent ? 'active' : ''}"
                 style="background:${a.hex}"
                 onclick="selectAccent(${i})"
                 title="${a.name}">
              <span class="accent-check">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none"
                     stroke="white" stroke-width="3">
                  <polyline points="20 6 9 17 4 12"/>
                </svg>
              </span>
            </div>
            <span class="accent-label">${a.name}</span>
          </div>`).join('')}
      </div>`;
  }
}

function selectBg(i) {
  currentBg = i;
  localStorage.setItem(LS_BG, i);
  document.body.classList.add('theme-transition');
  applyTheme();
  buildThemeModal('bg');
  setTimeout(() => document.body.classList.remove('theme-transition'), 500);
}

function selectAccent(i) {
  currentAccent = i;
  localStorage.setItem(LS_ACCENT, i);
  document.body.classList.add('theme-transition');
  applyTheme();
  buildThemeModal('accent');
  setTimeout(() => document.body.classList.remove('theme-transition'), 500);
}

function openThemeModal(tab) {
  const modal   = document.getElementById('themeModal');
  const overlay = document.getElementById('themeOverlay');
  if (!modal || !overlay) return;
  const title = document.getElementById('themeModalTitle');
  if (title) title.textContent = tab === 'bg' ? 'اختر الخلفية' : 'اختر لون التمييز';
  document.querySelectorAll('.theme-tab').forEach(t =>
    t.classList.toggle('active', t.dataset.tab === tab));
  buildThemeModal(tab);
  modal.classList.add('open');
  overlay.classList.add('open');
}

function closeThemeModal() {
  document.getElementById('themeModal')?.classList.remove('open');
  document.getElementById('themeOverlay')?.classList.remove('open');
}

/* ============================================================
   CSRF TOKEN
   ============================================================ */
function getCsrfToken() {
  return document.querySelector('meta[name="csrf-token"]')?.getAttribute('content')
      || document.querySelector('input[name="__RequestVerificationToken"]')?.value
      || '';
}

function fetchWithCsrf(url, options = {}) {
  const token = getCsrfToken();
  return fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      'X-CSRF-TOKEN': token,
      'RequestVerificationToken': token,
      ...options.headers,
    },
  });
}

/* ============================================================
   SEARCH BAR TOGGLE
   ============================================================ */
function toggleSearchBar() {
  const bar = document.getElementById('navbar-search-bar');
  if (!bar) return;
  const open = bar.classList.contains('show') || bar.classList.contains('open');
  if (open) {
    bar.classList.remove('show', 'open');
  } else {
    bar.classList.add('show', 'open');
    setTimeout(() => bar.querySelector('input')?.focus(), 50);
  }
}

/* ============================================================
   CART — Add
   ============================================================ */
async function addToCartAjax(variantId, quantity = 1) {
  try {
    const res  = await fetchWithCsrf('/cart/add', {
      method: 'POST',
      body: JSON.stringify({ variantId, quantity }),
    });
    const data = await res.json();
    if (data.success) {
      updateCartCount(data.cartCount);
      showToast(data.message || 'تم إضافة المنتج للسلة ✓', 'success');
    } else {
      showToast(data.message || 'حدث خطأ', 'danger');
    }
  } catch {
    showToast('حدث خطأ في الاتصال', 'danger');
  }
}

/* ============================================================
   CART — Update Quantity
   ============================================================ */
async function updateQuantityAjax(variantId, quantity) {
  try {
    const res  = await fetchWithCsrf('/cart/update', {
      method: 'POST',
      body: JSON.stringify({ variantId, quantity }),
    });
    const data = await res.json();
    if (data.success) {
      updateCartCount(data.cartCount);
      const el = document.querySelector(`[data-item-total="${variantId}"]`);
      if (el) el.textContent = Number(data.itemTotal).toLocaleString('ar-EG') + ' ج';

      _updateCartTotalsUI(data);

      const input = document.querySelector(`.qty-input[data-variant-id="${variantId}"]`);
      if (input) input.value = quantity;
    } else {
      showToast(data.message || 'الكمية المطلوبة غير متاحة', 'warning');
      setTimeout(() => location.reload(), 1500);
    }
  } catch {
    showToast('خطأ في تحديث الكمية', 'danger');
  }
}

/* ============================================================
   CART — Remove
   ============================================================ */
async function removeFromCartAjax(variantId) {
  if (!confirm('هل تريد حذف هذا المنتج من السلة؟')) return;
  try {
    const res  = await fetchWithCsrf(`/cart/remove/${variantId}`, { method: 'POST' });
    const data = await res.json();
    if (data.success) {
      document.querySelector(`[data-cart-row="${variantId}"]`)?.remove();
      updateCartCount(data.cartCount);
      _updateCartTotalsUI(data);
      if (data.cartCount === 0) location.reload();
      showToast('تم الحذف بنجاح', 'success');
    }
  } catch {
    showToast('خطأ في حذف المنتج', 'danger');
  }
}

async function clearCart() {
  if (!confirm('هل تريد تفريغ السلة بالكامل؟')) return;
  try {
    const res  = await fetchWithCsrf('/cart/clear', { method: 'POST' });
    const data = await res.json();
    if (data.success) { updateCartCount(0); location.reload(); }
  } catch { showToast('حدث خطأ', 'danger'); }
}

/** helper — تحديث عناصر الإجماليات في صفحة السلة */
function _updateCartTotalsUI(data) {
  const total = document.getElementById('cart-total');
  if (total) total.textContent = Number(data.cartTotal).toLocaleString('ar-EG') + ' جنيه';

  const subtotalEl = document.getElementById('cart-subtotal');
  if (subtotalEl && data.cartSubTotal !== undefined)
    subtotalEl.textContent = Number(data.cartSubTotal).toLocaleString('ar-EG') + ' جنيه';

  const discountEl = document.getElementById('cart-discount');
  if (discountEl) {
    const row = discountEl.closest('.premium-summary-row');
    if (data.cartDiscount !== undefined && Number(data.cartDiscount) > 0) {
      discountEl.textContent = '− ' + Number(data.cartDiscount).toLocaleString('ar-EG') + ' جنيه';
      if (row) row.style.display = 'flex';
      else discountEl.style.display = '';
    } else {
      if (row) row.style.display = 'none';
      else discountEl.style.display = 'none';
    }
  }
}

/* ============================================================
   CART COUNT Badge
   ============================================================ */
function updateCartCount(count) {
  document.querySelectorAll('.cart-count-badge, .navbar-badge[data-badge="cart"]').forEach(el => {
    el.textContent    = count;
    el.style.display  = count > 0 ? 'flex' : 'none';
  });
  localStorage.setItem('cartCount', count);
}

/* ============================================================
   WISHLIST
   ============================================================ */
async function toggleWishlist(productId) {
  try {
    const res = await fetchWithCsrf(`/wishlist/toggle/${productId}`, { method: 'POST' });
    if (res.redirected || res.status === 401) { location.href = '/account/login'; return; }
    const data = await res.json();
    if (data.success) {
      // Bootstrap icons style
      document.querySelectorAll(`.wishlist-btn[data-product-id="${productId}"] i`).forEach(icon => {
        icon.className = data.isAdded ? 'bi bi-heart-fill' : 'bi bi-heart';
      });
      // card-wishlist button style (ecommerce front)
      document.querySelectorAll(`.card-wishlist[data-product-id="${productId}"]`).forEach(btn => {
        btn.classList.toggle('active', data.isAdded);
      });
      document.querySelectorAll('.wishlist-count-badge, .wishlist-count').forEach(el => {
        el.textContent   = data.wishlistCount;
        el.style.display = data.wishlistCount > 0 ? 'flex' : 'none';
      });
      showToast(data.isAdded ? 'تم إضافته للمفضلة ❤️' : 'تم حذفه من المفضلة', 'info');
    }
  } catch { showToast('يجب تسجيل الدخول أولاً', 'warning'); }
}

/* ============================================================
   NOTIFICATIONS
   ============================================================ */
async function updateNotificationCount() {
  try {
    const res  = await fetch('/notifications/count');
    if (!res.ok) return;
    const data = await res.json();
    document.querySelectorAll('.notification-count-badge').forEach(el => {
      el.textContent   = data.count;
      el.style.display = data.count > 0 ? 'flex' : 'none';
    });
  } catch {}
}

async function loadNotificationDropdown() {
  try {
    const res  = await fetch('/notifications/dropdown');
    const html = await res.text();
    const c    = document.getElementById('notification-dropdown-content');
    if (c) c.innerHTML = html;
  } catch {}
}

async function markRead(id) {
  try { await fetchWithCsrf(`/notifications/markread/${id}`, { method: 'POST' }); } catch {}
}

async function markAllRead() {
  try {
    await fetchWithCsrf('/notifications/markallread', { method: 'POST' });
    document.querySelectorAll('.notif-card.unread, .notif-item.unread').forEach(el => {
      el.classList.remove('unread', 'is-unread');
      el.querySelector('.tella-notif-new')?.remove();
      el.querySelector('.tella-notif-dot')?.remove();
    });
    document.querySelectorAll('.notification-count-badge').forEach(el => {
      el.textContent   = '0';
      el.style.display = 'none';
    });
  } catch {}
}

/* ============================================================
   TOAST
   ============================================================ */
let toastCounter = 0;
function showToast(message, type = 'success') {
  let container = document.getElementById('toast-container');
  if (!container) {
    container    = document.createElement('div');
    container.id = 'toast-container';
    document.body.appendChild(container);
  }
  const icons = { success: '✓', danger: '✕', warning: '⚠', info: 'ℹ' };
  const id     = 'toast-' + (++toastCounter);
  const div    = document.createElement('div');
  div.id        = id;
  div.className = `toast-item toast-${type}`;
  div.innerHTML = `
    <div class="toast-icon">${icons[type] || 'ℹ'}</div>
    <div class="toast-msg">${message}</div>
    <button class="toast-close" onclick="document.getElementById('${id}').remove()">✕</button>`;
  container.appendChild(div);
  setTimeout(() => document.getElementById(id)?.remove(), 4000);
}

/* ============================================================
   PRODUCT DETAILS — GALLERY & QTY
   ============================================================ */
window.switchMainImage = (url, thumbEl) => {
  const main = document.getElementById('mainProductImage');
  if (main) main.src = url;
  document.querySelectorAll('.thumb-item, .img-thumb').forEach(t => t.classList.remove('active'));
  (thumbEl || event?.currentTarget)?.classList.add('active');
};

window.changeQty = (delta) => {
  const input = document.getElementById('product-qty');
  if (!input) return;
  let val = parseInt(input.value) + delta;
  if (val < 1)  val = 1;
  if (val > 99) val = 99;
  input.value = val;
};

/* ============================================================
   RECENTLY VIEWED
   ============================================================ */
const RECENTLY_VIEWED_KEY = 'tella_recently_viewed';
const RECENTLY_VIEWED_MAX = 6;

function addToRecentlyViewed(productId) {
  try {
    let ids = JSON.parse(localStorage.getItem(RECENTLY_VIEWED_KEY) || '[]');
    ids = ids.filter(id => id !== productId);
    ids.unshift(productId);
    ids = ids.slice(0, RECENTLY_VIEWED_MAX);
    localStorage.setItem(RECENTLY_VIEWED_KEY, JSON.stringify(ids));
  } catch {}
}

async function loadRecentlyViewed(currentProductId) {
  const section = document.getElementById('recently-viewed-section');
  if (!section) return;
  try {
    let ids = JSON.parse(localStorage.getItem(RECENTLY_VIEWED_KEY) || '[]');
    ids = ids.filter(id => id !== currentProductId);
    if (ids.length === 0) { section.remove(); return; }
    const res = await fetch(`/products/recently-viewed?ids=${ids.join(',')}`);
    if (!res.ok) return;
    const html = await res.text();
    if (!html.trim()) { section.remove(); return; }
    section.outerHTML = html;
  } catch { section.remove(); }
}

/* ============================================================
   IN-VIEW ANIMATIONS (Intersection Observer)
   ============================================================ */
function initInViewAnimations() {
  const els = document.querySelectorAll('.pre-animate');
  if (!els.length || !window.IntersectionObserver) return;

  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('in-view');
        entry.target.classList.remove('pre-animate');
        observer.unobserve(entry.target);
      }
    });
  }, { threshold: 0.15 });

  els.forEach(el => observer.observe(el));
}

/* ============================================================
   DOMContentLoaded — تهيئة كل شيء بعد تحميل الصفحة
   ============================================================ */
document.addEventListener('DOMContentLoaded', async () => {

  /* --- Theme picker events --- */
  document.getElementById('bgPickerBtn')
    ?.addEventListener('click', () => openThemeModal('bg'));
  document.getElementById('accentPickerBtn')
    ?.addEventListener('click', () => openThemeModal('accent'));
  document.getElementById('themeModalClose')
    ?.addEventListener('click', closeThemeModal);
  document.getElementById('themeOverlay')
    ?.addEventListener('click', closeThemeModal);

  document.querySelectorAll('.theme-tab').forEach(tab => {
    tab.addEventListener('click', () => {
      document.querySelectorAll('.theme-tab').forEach(t => t.classList.remove('active'));
      tab.classList.add('active');
      const t = document.getElementById('themeModalTitle');
      if (t) t.textContent = tab.dataset.tab === 'bg' ? 'اختر الخلفية' : 'اختر لون التمييز';
      buildThemeModal(tab.dataset.tab);
    });
  });

  /* --- Cart count on page load --- */
  try {
    const res = await fetch('/cart/count');
    if (res.ok) {
      const data = await res.json();
      updateCartCount(data.count);
    }
  } catch {
    const saved = localStorage.getItem('cartCount');
    if (saved && parseInt(saved) > 0) updateCartCount(parseInt(saved));
  }

  /* --- Notifications polling --- */
  if (document.querySelector('.notification-count-badge')) {
    updateNotificationCount();
    setInterval(updateNotificationCount, 30000);
  }

  /* --- Mobile menu toggle --- */
  document.querySelectorAll('[data-mobile-menu-toggle]').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelector('.mobile-menu')?.classList.toggle('open');
    });
  });

  /* --- Search close button --- */
  document.querySelector('.search-close')?.addEventListener('click', () => {
    document.getElementById('navbar-search-bar')?.classList.remove('show', 'open');
  });

  /* --- In-view animations --- */
  initInViewAnimations();

  /* --- Keyboard: close theme modal on Escape --- */
  document.addEventListener('keydown', e => {
    if (e.key === 'Escape') closeThemeModal();
  });
});
