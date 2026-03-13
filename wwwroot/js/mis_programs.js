function toggleNode(element) {
    const treeItem = element;
    const treeNode = element.parentElement;
    const children = treeNode.querySelector('.tree-children');

    if (!children) return;

    if (children.style.display === 'none') {
        children.style.display = 'block';
        treeItem.classList.add('expanded');
    } else {
        children.style.display = 'none';
        treeItem.classList.remove('expanded');
    }
}

function toDateOnly(value) {
    if (!value || typeof value === 'object' || value === '-') return '-';
    const s = String(value);
    if (s.includes('T')) return s.split('T')[0];
    if (s.includes(' ')) return s.split(' ')[0];
    return s;
}

let __modalScrollLockCount = 0;
let __modalScrollTop = 0;
let __executionModalState = {
    id: '',
    url: '',
    title: '',
    code: ''
};
let __executionSequence = 0;
let __minimizedExecutions = [];

function lockBackgroundScroll() {
    __modalScrollLockCount += 1;
    if (__modalScrollLockCount > 1) return;

    __modalScrollTop = window.scrollY || document.documentElement.scrollTop || 0;
    document.documentElement.style.overflow = 'hidden';
    document.body.style.overflow = 'hidden';
    document.body.style.position = 'fixed';
    document.body.style.top = `-${__modalScrollTop}px`;
    document.body.style.left = '0';
    document.body.style.right = '0';
    document.body.style.width = '100%';
    document.body.classList.add('g-modal-open');
}

function unlockBackgroundScroll() {
    if (__modalScrollLockCount <= 0) {
        __modalScrollLockCount = 0;
        return;
    }
    __modalScrollLockCount -= 1;
    if (__modalScrollLockCount > 0) return;

    document.documentElement.style.overflow = '';
    document.body.style.overflow = '';
    document.body.style.position = '';
    document.body.style.top = '';
    document.body.style.left = '';
    document.body.style.right = '';
    document.body.style.width = '';
    document.body.classList.remove('g-modal-open');
    window.scrollTo(0, __modalScrollTop);
}

function stopBackdropScroll(event) {
    const programModal = document.getElementById('programModal');
    const execModal = document.getElementById('executionModal');
    const target = event.target;

    const inProgramContent = target?.closest?.('#modalContent');
    const inExecContent = target?.closest?.('#executionModalContent');
    const programVisible = programModal && !programModal.classList.contains('hidden') && programModal.style.display !== 'none';
    const execVisible = execModal && !execModal.classList.contains('hidden') && execModal.style.display !== 'none';

    if ((programVisible || execVisible) && !inProgramContent && !inExecContent) {
        event.preventDefault();
    }
}

document.addEventListener('wheel', stopBackdropScroll, { passive: false });
document.addEventListener('touchmove', stopBackdropScroll, { passive: false });

function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
}

function collectMisProgramFilters() {
    return {
        program_no: (document.getElementById('program_no')?.value || '').trim(),
        employee_id: (document.getElementById('employee_id')?.value || '').trim(),
        display_code: (document.getElementById('display_code')?.value || '').trim()
    };
}

function renderMisProgramTree(categories) {
    const tree = document.getElementById('misProgramsTree');
    if (!tree) return;

    if (!Array.isArray(categories) || categories.length === 0) {
        tree.innerHTML = '<div class="text-slate-500 text-sm">查無資料</div>';
        return;
    }

    tree.innerHTML = categories.map(category => `
        <div class="tree-node mb-2">
            <button type="button" class="tree-item system-level w-full text-left" onclick="toggleNode(this)">
                <svg class="arrow w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>
                <svg class="folder-icon w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7a2 2 0 012-2h4l2 2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2V7z" /></svg>
                <span class="tree-label">${escapeHtml(category.name)} (${category.count ?? 0})</span>
            </button>
            <div class="tree-children hidden">
                ${(category.programs || []).map(program => `
                    <button type="button"
                            class="tree-item program-level w-full text-left"
                            onclick="openProgramModal(this)"
                            data-program-no="${escapeHtml(program.programNo)}"
                            data-program-name="${escapeHtml(program.programName)}"
                            data-purpose="${escapeHtml(program.purpose)}"
                            data-employee-id="${escapeHtml(program.employeeId)}"
                            data-program-type="${escapeHtml(program.programType)}"
                            data-plan-start="${escapeHtml(program.planStart)}"
                            data-plan-finish="${escapeHtml(program.planFinish)}"
                            data-real-start="${escapeHtml(program.realStart)}"
                            data-real-finish="${escapeHtml(program.realFinish)}"
                            data-plan-hours="${escapeHtml(program.planHours)}"
                            data-real-hours="${escapeHtml(program.realHours)}">
                        <svg class="file-icon w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
                        <span class="program-no">${escapeHtml(program.programNo)}</span>
                        <span class="program-purpose">${escapeHtml(program.programName)}</span>
                        <span class="mis-status-pill ${(String(program.displayCode || '').toUpperCase() === 'Y') ? 'y' : 'n'}">${String(program.displayCode || '').toUpperCase() === 'Y' ? '啟用' : '停用'}</span>
                    </button>
                `).join('')}
            </div>
        </div>
    `).join('');
}

function setMisProgramsError(message = '') {
    const box = document.getElementById('misProgramsError');
    if (!box) return;

    if (!message) {
        box.classList.add('hidden');
        box.innerHTML = '';
        return;
    }

    box.classList.remove('hidden');
    box.innerHTML = `<p>${escapeHtml(message)}</p>`;
}

async function fetchMisPrograms(filters, options = {}) {
    const queryButton = document.getElementById('misProgramsQueryButton');
    const clearButton = document.getElementById('misProgramsClearButton');
    const runButton = document.getElementById('misProgramsRunButton');
    const url = new URL('/api/mis/programs/search', window.location.origin);
    const safeFilters = {
        program_no: filters?.program_no || '',
        employee_id: filters?.employee_id || '',
        display_code: filters?.display_code || 'Y'
    };

    Object.entries(safeFilters).forEach(([key, value]) => {
        if (value !== '') url.searchParams.set(key, value);
    });

    [queryButton, clearButton, runButton].forEach(btn => btn?.setAttribute('disabled', 'disabled'));

    try {
        const response = await fetch(url.toString(), { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (response.status === 401) {
            window.location.href = '/Account/Login';
            return;
        }

        const result = await response.json();
        if (!response.ok || result?.status !== 'success') {
            setMisProgramsError(result?.message || `查詢失敗 (${response.status})`);
            return;
        }

        renderMisProgramTree(result.categories || []);
        const countNode = document.getElementById('misProgramsCount');
        if (countNode) {
            countNode.textContent = String(result.count ?? 0);
        }
        setMisProgramsError('');

        if (options.pushState !== false) {
            const pageUrl = new URL('/mis/programs', window.location.origin);
            Object.entries(safeFilters).forEach(([key, value]) => {
                if (value !== '') pageUrl.searchParams.set(key, value);
            });
            window.history.replaceState({}, '', pageUrl.pathname + pageUrl.search);
        }
    } catch (error) {
        setMisProgramsError(error?.message || '查詢失敗');
    } finally {
        [queryButton, clearButton, runButton].forEach(btn => btn?.removeAttribute('disabled'));
    }
}

function clearMisProgramsFilters() {
    const programNo = document.getElementById('program_no');
    const employeeId = document.getElementById('employee_id');
    const employeeCode = document.getElementById('employee_id_code');
    const employeeName = document.getElementById('employee_id_name');
    const employeeDisplay = document.getElementById('employee_id_name_display') || document.getElementById('employee_id_code_display');
    const displayCode = document.getElementById('display_code');

    if (programNo) programNo.value = '';
    if (employeeId) employeeId.value = '';
    if (employeeCode) employeeCode.value = '';
    if (employeeName) employeeName.value = '';
    if (employeeDisplay) employeeDisplay.value = '';
    if (displayCode) displayCode.value = 'Y';

    fetchMisPrograms({ program_no: '', employee_id: '', display_code: 'Y' });
}

function initMisProgramsQuery() {
    const form = document.getElementById('misProgramsQueryForm');
    const clearButton = document.getElementById('misProgramsClearButton');
    if (!form) return;

    form.addEventListener('submit', (event) => {
        event.preventDefault();
        fetchMisPrograms(collectMisProgramFilters());
    });

    clearButton?.addEventListener('click', (event) => {
        event.preventDefault();
        clearMisProgramsFilters();
    });
}

function openProgramModal(input) {
    let data;

    if (input instanceof HTMLElement) {
        data = {
            programNo: input.dataset.programNo,
            programName: input.dataset.programName,
            purpose: input.dataset.purpose,
            employeeId: input.dataset.employeeId,
            programType: input.dataset.programType,
            planStart: input.dataset.planStart,
            planFinish: input.dataset.planFinish,
            realStart: input.dataset.realStart,
            realFinish: input.dataset.realFinish,
            planHours: input.dataset.planHours,
            realHours: input.dataset.realHours
        };
    } else {
        data = {
            programNo: input.PROGRAM_NO,
            programName: input.PROGRAM_NAME,
            purpose: input.PURPOSE,
            employeeId: input.EMPLOYEE_ID,
            programType: input.PROGRAM_TYPE,
            planStart: input.PLAN_START_DEVELOP_DATE,
            planFinish: input.PLAN_FINISH_DEVELOP_DATE,
            realStart: input.REAL_START_DEVELOP_DATE,
            realFinish: input.REAL_FINISH_DEVELOP_DATE,
            planHours: input.PLAN_WORK_HOURS,
            realHours: input.REAL_WORK_HOURS
        };
    }

    const targetProgramNo = String(data.programNo || '').trim().toUpperCase();
    const minimizedMatch = __minimizedExecutions.find(item => item.code === targetProgramNo);
    if (minimizedMatch) {
        restoreExecutionModal(minimizedMatch.id);
        return;
    }

    document.getElementById('modal-program-no').textContent = data.programNo || '-';
    document.getElementById('modal-program-name').textContent = data.programName || '-';
    document.getElementById('modal-purpose').textContent = data.purpose || '-';
    document.getElementById('modal-employee-id').textContent = data.employeeId || '-';
    document.getElementById('modal-program-type').textContent = data.programType || '-';

    document.getElementById('modal-plan-start').textContent = toDateOnly(data.planStart);
    document.getElementById('modal-plan-finish').textContent = toDateOnly(data.planFinish);
    document.getElementById('modal-real-start').textContent = toDateOnly(data.realStart);
    document.getElementById('modal-real-finish').textContent = toDateOnly(data.realFinish);

    document.getElementById('modal-plan-hours').textContent = data.planHours || '-';
    document.getElementById('modal-real-hours').textContent = data.realHours || '-';

    const btnOpenProgram = document.getElementById('btnOpenProgram');
    if (btnOpenProgram) {
        btnOpenProgram.onclick = function (e) {
            e.preventDefault();
            const programNo = (data.programNo || '').toUpperCase();
            openExecutionModal(resolveProgramUrl(programNo), `${programNo} ${data.programName || ''}`.trim());
        };
    }

    const modal = document.getElementById('programModal');
    modal.style.display = 'flex';
    modal.classList.remove('hidden');
    modal.style.overscrollBehavior = 'contain';

    const content = document.getElementById('modalContent');
    content.style.overscrollBehavior = 'contain';
    if (!content.classList.contains('w-screen')) {
        toggleMaximizeModal();
    }

    const modalBody = modal.querySelector('.overflow-y-auto');
    const header = modal.querySelector('.bg-blue-600');
    if (modalBody && header) {
        modalBody.onscroll = function () {
            if (modalBody.scrollTop > 10) {
                header.classList.add('shadow-xl');
            } else {
                header.classList.remove('shadow-xl');
            }
        };
    }

    lockBackgroundScroll();
}

function closeProgramModal(unlockScroll = true) {
    const modal = document.getElementById('programModal');
    const content = document.getElementById('modalContent');
    const wasVisible = modal.style.display !== 'none' && !modal.classList.contains('hidden');

    modal.style.display = 'none';
    modal.classList.add('hidden');
    if (unlockScroll && wasVisible) {
        unlockBackgroundScroll();
    }

    if (content.classList.contains('w-screen')) {
        toggleMaximizeModal();
    }
}

function toggleMaximizeModal() {
    const modal = document.getElementById('programModal');
    const content = document.getElementById('modalContent');
    const maxIcon = document.getElementById('maximizeIcon');
    const restoreIcon = document.getElementById('restoreIcon');
    const isMax = content.classList.contains('w-screen');

    if (isMax) {
        content.classList.remove('w-screen', 'h-screen', 'max-w-none', 'max-h-none', 'rounded-none', 'scale-100');
        content.classList.add('w-full', 'max-w-4xl', 'max-h-[90vh]', 'rounded-2xl', 'scale-95');
        modal.classList.add('p-4');
        maxIcon.classList.remove('hidden');
        restoreIcon.classList.add('hidden');
    } else {
        content.classList.add('w-screen', 'h-screen', 'max-w-none', 'max-h-none', 'rounded-none', 'scale-100');
        content.classList.remove('w-full', 'max-w-4xl', 'max-h-[90vh]', 'rounded-2xl', 'scale-95');
        modal.classList.remove('p-4');
        maxIcon.classList.add('hidden');
        restoreIcon.classList.remove('hidden');
    }
}

window.onclick = function (event) {
    const modal = document.getElementById('programModal');
    if (modal && event.target === modal) {
        closeProgramModal();
    }
};

function runProgram() {
    const input = document.getElementById('program_no') || document.getElementsByName('program_no')[0];
    const programNo = (input?.value || '').trim().toUpperCase();

    if (!programNo) {
        alert('請先輸入程式編號');
        return;
    }

    openExecutionModal(resolveProgramUrl(programNo), programNo);
}

function resolveProgramUrl(programNo) {
    const code = String(programNo || '').trim().toUpperCase();
    if (!code) return '/mis/programs';

    const moduleMatch = code.match(/^([A-Z]{3})[A-Z0-9_]+$/);
    if (moduleMatch) {
        const module = moduleMatch[1].toLowerCase();
        const controller = module.charAt(0).toUpperCase() + module.slice(1);
        return `/${controller}/${code}`;
    }

    return `/mis/programs/${encodeURIComponent(code)}`;
}

function openExecutionModal(url, title) {
    const modal = document.getElementById('executionModal');
    const titleEl = document.getElementById('executionModalTitle');
    const modalContent = document.getElementById('executionModalContent');
    const maxIcon = document.getElementById('execMaximizeIcon');
    const restoreIcon = document.getElementById('execRestoreIcon');
    const normalizedUrl = String(url || '').trim();
    const code = String(title || '').trim().toUpperCase().split(/[\s-]+/)[0];

    const minimizedMatch = __minimizedExecutions.find(item => item.url === normalizedUrl);
    if (minimizedMatch) {
        restoreExecutionModal(minimizedMatch.id);
        closeProgramModal();
        return;
    }

    const iframe = ensureActiveExecutionIframe();
    __executionModalState.id = `exec_${++__executionSequence}`;
    __executionModalState.url = normalizedUrl;
    __executionModalState.title = title ? `${title} - 程式執行` : '程式執行';
    __executionModalState.code = code;

    titleEl.textContent = __executionModalState.title;
    if (iframe.src !== normalizedUrl) {
        iframe.src = normalizedUrl;
    }
    renderExecutionMinimizedDock();

    modal.classList.remove('p-0');
    modal.classList.add('p-4');
    modalContent.classList.remove('w-full', 'h-screen', 'rounded-none', 'max-w-none', 'max-h-none');
    modalContent.classList.add('w-full', 'max-w-7xl', 'rounded-2xl');
    if (maxIcon && restoreIcon) {
        maxIcon.classList.remove('hidden');
        restoreIcon.classList.add('hidden');
    }

    setExecutionModalChrome({ hideHeader: false });

    modal.classList.remove('hidden');
    modal.style.display = 'flex';
    modal.style.overscrollBehavior = 'contain';
    lockBackgroundScroll();

    setTimeout(() => {
        modalContent.classList.remove('scale-95');
        modalContent.classList.add('scale-100');
        modalContent.style.overscrollBehavior = 'contain';
    }, 10);

    closeProgramModal();
}

function closeExecutionModal() {
    const modal = document.getElementById('executionModal');
    const modalContent = document.getElementById('executionModalContent');
    const iframe = document.getElementById('executionIframe');

    modalContent.classList.remove('scale-100');
    modalContent.classList.add('scale-95');

    setTimeout(() => {
        modal.classList.add('hidden');
        modal.style.display = 'none';
        if (iframe) {
            iframe.src = 'about:blank';
            iframe.remove();
        }
        __executionModalState.id = '';
        __executionModalState.url = '';
        __executionModalState.title = '';
        __executionModalState.code = '';
        unlockBackgroundScroll();
    }, 300);
}

function setExecutionModalChrome(options = {}) {
    const modalContent = document.getElementById('executionModalContent');
    const modalHeader = document.getElementById('executionModalHeader');
    if (!modalContent || !modalHeader) return;

    const hideHeader = !!options.hideHeader;
    const isMaximized = modalContent.classList.contains('h-screen');

    if (hideHeader) {
        modalHeader.classList.add('hidden');
    } else {
        modalHeader.classList.remove('hidden');
    }

    if (!isMaximized) {
        modalContent.classList.remove('h-[95vh]', 'h-[98vh]');
        modalContent.classList.add(hideHeader ? 'h-[98vh]' : 'h-[95vh]');
    }
}

function ensureActiveExecutionIframe() {
    const modalContent = document.getElementById('executionModalContent');
    const existing = document.getElementById('executionIframe');
    if (existing) return existing;

    const iframe = document.createElement('iframe');
    iframe.id = 'executionIframe';
    iframe.className = 'flex-1 w-full border-0';
    iframe.src = 'about:blank';
    iframe.title = '程式執行視窗';
    modalContent.appendChild(iframe);
    return iframe;
}

function getExecutionParking() {
    return document.getElementById('executionIframeParking');
}

function renderExecutionMinimizedDock() {
    const dockList = document.getElementById('executionMinimizedDockList');
    if (!dockList) return;

    if (!__minimizedExecutions.length) {
        dockList.innerHTML = '';
        dockList.classList.add('hidden');
        dockList.style.display = 'none';
        return;
    }

    dockList.innerHTML = __minimizedExecutions.map(item => `
        <button type="button"
                class="inline-flex items-center gap-3 rounded-full bg-blue-700 px-4 py-3 text-white shadow-2xl hover:bg-blue-800 transition-colors"
                onclick="restoreExecutionModal('${item.id}')"
                title="${item.title}">
            <svg class="h-4 w-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8V4h4M20 8V4h-4M4 16v4h4M20 16v4h-4" />
            </svg>
            <span class="max-w-56 truncate text-sm font-semibold">${item.title}</span>
        </button>
    `).join('');

    dockList.classList.remove('hidden');
    dockList.classList.add('flex');
    dockList.style.display = 'flex';
}

function minimizeExecutionModal() {
    const modal = document.getElementById('executionModal');
    const modalContent = document.getElementById('executionModalContent');
    const iframe = document.getElementById('executionIframe');
    const parking = getExecutionParking();
    if (!modal || modal.classList.contains('hidden') || !iframe || !parking || !__executionModalState.id) return;

    modalContent.classList.remove('scale-100');
    modalContent.classList.add('scale-95');

    setTimeout(() => {
        iframe.removeAttribute('id');
        iframe.dataset.executionId = __executionModalState.id;
        iframe.classList.add('hidden');
        parking.appendChild(iframe);

        __minimizedExecutions = __minimizedExecutions.filter(item => item.id !== __executionModalState.id);
        __minimizedExecutions.push({
            id: __executionModalState.id,
            url: __executionModalState.url,
            title: __executionModalState.title || '程式執行',
            code: __executionModalState.code
        });

        modal.classList.add('hidden');
        modal.style.display = 'none';
        renderExecutionMinimizedDock();
        __executionModalState.id = '';
        __executionModalState.url = '';
        __executionModalState.title = '';
        __executionModalState.code = '';
        unlockBackgroundScroll();
    }, 180);
}

function restoreExecutionModal(executionId = '') {
    const modal = document.getElementById('executionModal');
    const modalContent = document.getElementById('executionModalContent');
    const titleEl = document.getElementById('executionModalTitle');
    const parking = getExecutionParking();
    const targetId = executionId || __minimizedExecutions.at(-1)?.id || '';
    if (!modal || !parking || !targetId) return;

    const target = __minimizedExecutions.find(item => item.id === targetId);
    const iframe = parking.querySelector(`iframe[data-execution-id="${targetId}"]`);
    if (!target || !iframe) return;

    __executionModalState.id = target.id;
    __executionModalState.url = target.url;
    __executionModalState.title = target.title;
    __executionModalState.code = target.code;
    if (titleEl) {
        titleEl.textContent = __executionModalState.title || '程式執行';
    }

    iframe.classList.remove('hidden');
    iframe.id = 'executionIframe';
    modalContent.appendChild(iframe);

    __minimizedExecutions = __minimizedExecutions.filter(item => item.id !== targetId);
    renderExecutionMinimizedDock();
    modal.classList.remove('hidden');
    modal.style.display = 'flex';
    lockBackgroundScroll();

    setTimeout(() => {
        modalContent.classList.remove('scale-95');
        modalContent.classList.add('scale-100');
    }, 10);
}

function setExecutionModalWindowMode(mode) {
    const modal = document.getElementById('executionModal');
    const modalContent = document.getElementById('executionModalContent');
    const modalHeader = document.getElementById('executionModalHeader');
    const maxIcon = document.getElementById('execMaximizeIcon');
    const restoreIcon = document.getElementById('execRestoreIcon');
    const isMaximized = modalContent.classList.contains('h-screen');
    const isHeaderHidden = modalHeader?.classList.contains('hidden');
    const targetMode = mode === 'toggle' ? (isMaximized ? 'normal' : 'max') : mode;

    if (targetMode === 'max' && !isMaximized) {
        modal.classList.remove('p-4');
        modal.classList.add('p-0');
        modalContent.classList.remove('max-w-7xl', 'h-[95vh]', 'h-[98vh]', 'rounded-2xl');
        modalContent.classList.add('w-full', 'h-screen', 'max-w-none', 'max-h-none', 'rounded-none');
        if (maxIcon && restoreIcon) {
            maxIcon.classList.add('hidden');
            restoreIcon.classList.remove('hidden');
        }
        return;
    }

    if (targetMode === 'normal' && isMaximized) {
        modal.classList.remove('p-0');
        modal.classList.add('p-4');
        modalContent.classList.remove('w-full', 'h-screen', 'max-w-none', 'max-h-none', 'rounded-none');
        modalContent.classList.add('w-full', 'max-w-7xl', 'rounded-2xl');
        modalContent.classList.remove('h-[95vh]', 'h-[98vh]');
        modalContent.classList.add(isHeaderHidden ? 'h-[98vh]' : 'h-[95vh]');
        if (maxIcon && restoreIcon) {
            maxIcon.classList.remove('hidden');
            restoreIcon.classList.add('hidden');
        }
    }
}

function toggleExecutionMaximize() {
    setExecutionModalWindowMode('toggle');
}

window.setExecutionModalWindowMode = setExecutionModalWindowMode;
window.setExecutionModalChrome = setExecutionModalChrome;
window.minimizeExecutionModal = minimizeExecutionModal;
window.restoreExecutionModal = restoreExecutionModal;
document.addEventListener('DOMContentLoaded', initMisProgramsQuery);

