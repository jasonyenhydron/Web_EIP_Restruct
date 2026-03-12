
function eipPanelToggle(panelId) {
    const body = document.getElementById(panelId);
    const arrow = document.getElementById(`${panelId}-arrow`);
    if (!body) return;

    const isHidden = body.classList.contains('hidden');
    body.classList.toggle('hidden', !isHidden);
    if (arrow) arrow.classList.toggle('rotate-180', !isHidden);
}

function eipDialogOpen(dialogId) {
    const dlg = document.getElementById(dialogId);
    const content = document.getElementById(`${dialogId}-content`);
    if (!dlg) return;

    dlg.style.display = 'flex';
    dlg.classList.remove('hidden');
    document.body.style.overflow = 'hidden';

    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            if (content) {
                content.classList.remove('scale-95', 'opacity-0');
                content.classList.add('scale-100', 'opacity-100');
            }
        });
    });
}

function eipDialogClose(dialogId) {
    const dlg = document.getElementById(dialogId);
    const content = document.getElementById(`${dialogId}-content`);
    if (!dlg) return;

    if (content) {
        content.classList.remove('scale-100', 'opacity-100');
        content.classList.add('scale-95', 'opacity-0');
    }

    setTimeout(() => {
        dlg.style.display = 'none';
        dlg.classList.add('hidden');
        document.body.style.overflow = '';
    }, 180);
}

function eipToast(message, type = 'success') {
    const colors = {
        success: 'bg-blue-600',
        error: 'bg-slate-100',
        warning: 'bg-slate-100',
        info: 'bg-blue-600'
    };
    const icons = {
        success: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>',
        error: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>',
        warning: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>',
        info: '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>'
    };

    document.querySelectorAll('.eip-toast').forEach((el) => el.remove());

    const toast = document.createElement('div');
    toast.className = `eip-toast fixed bottom-5 right-5 z-[999] flex items-center gap-3 px-5 py-3.5 rounded-xl shadow-2xl text-white text-sm font-semibold transition-all duration-300 ${colors[type] ?? colors.info} opacity-0 translate-y-4`;
    toast.innerHTML = `
        <svg class="w-5 h-5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            ${icons[type] ?? icons.info}
        </svg>
        <span>${message}</span>
    `;

    document.body.appendChild(toast);

    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            toast.classList.remove('opacity-0', 'translate-y-4');
            toast.classList.add('opacity-100', 'translate-y-0');
        });
    });

    setTimeout(() => {
        toast.classList.remove('opacity-100', 'translate-y-0');
        toast.classList.add('opacity-0', 'translate-y-4');
        setTimeout(() => toast.remove(), 350);
    }, 2800);
}

function eipConfirm(message, title = '系統提示') {
    return new Promise((resolve) => {
        const id = `_eipConfirm_${Date.now()}`;
        const overlay = document.createElement('div');
        overlay.id = id;
        overlay.className = 'fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-[900] flex items-center justify-center p-4';
        overlay.innerHTML = `
            <div class="bg-white rounded-2xl shadow-2xl border border-slate-200 w-full max-w-sm transform scale-95 opacity-0 transition-all duration-200" id="${id}-box">
                <div class="px-5 pt-5 pb-4">
                    <div class="flex items-center gap-3 mb-3">
                        <div class="w-10 h-10 rounded-full bg-slate-100 flex items-center justify-center shrink-0">
                            <svg class="w-5 h-5 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
                            </svg>
                        </div>
                        <h3 class="text-base font-bold text-slate-800">${title}</h3>
                    </div>
                    <p class="text-sm text-slate-600 leading-relaxed">${message}</p>
                </div>
                <div class="flex justify-end gap-2 px-5 pb-5">
                    <button id="${id}-cancel" class="px-4 py-2 text-sm font-semibold rounded-lg bg-slate-100 text-slate-700 transition-colors">取消</button>
                    <button id="${id}-ok" class="px-4 py-2 text-sm font-semibold rounded-lg bg-blue-600 text-white transition-colors">確定</button>
                </div>
            </div>
        `;
        document.body.appendChild(overlay);

        const box = document.getElementById(`${id}-box`);
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                box.classList.remove('scale-95', 'opacity-0');
                box.classList.add('scale-100', 'opacity-100');
            });
        });

        const close = (result) => {
            box.classList.add('scale-95', 'opacity-0');
            setTimeout(() => overlay.remove(), 180);
            resolve(result);
        };

        document.getElementById(`${id}-ok`).onclick = () => close(true);
        document.getElementById(`${id}-cancel`).onclick = () => close(false);
        overlay.onclick = (e) => {
            if (e.target === overlay) close(false);
        };
    });
}

