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
    const iframe = document.getElementById('executionIframe');
    const titleEl = document.getElementById('executionModalTitle');
    const modalContent = document.getElementById('executionModalContent');
    const modalHeader = document.getElementById('executionModalHeader');
    const maxIcon = document.getElementById('execMaximizeIcon');
    const restoreIcon = document.getElementById('execRestoreIcon');

    const code = String(title || '').trim().toUpperCase().split(/[\s-]+/)[0];
    const hideOuterHeader = code === 'IDMGD01' || String(url || '').toUpperCase().includes('/IDM/IDMGD01');

    titleEl.textContent = title ? `${title} - 程式執行` : '程式執行';
    iframe.src = url;

    modal.classList.remove('p-0');
    modal.classList.add('p-4');
    modalContent.classList.remove('w-full', 'h-screen', 'rounded-none', 'max-w-none', 'max-h-none');
    modalContent.classList.add('w-full', 'max-w-7xl', 'rounded-2xl');
    if (maxIcon && restoreIcon) {
        maxIcon.classList.remove('hidden');
        restoreIcon.classList.add('hidden');
    }

    if (modalHeader && modalContent) {
        if (hideOuterHeader) {
            modalHeader.classList.add('hidden');
            modalContent.classList.remove('h-screen', 'h-[95vh]');
            modalContent.classList.add('h-[98vh]');
        } else {
            modalHeader.classList.remove('hidden');
            modalContent.classList.remove('h-screen', 'h-[98vh]');
            modalContent.classList.remove('h-[98vh]');
            modalContent.classList.add('h-[95vh]');
        }
    }

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
    const iframe = document.getElementById('executionIframe');
    const modalContent = document.getElementById('executionModalContent');

    modalContent.classList.remove('scale-100');
    modalContent.classList.add('scale-95');

    setTimeout(() => {
        modal.classList.add('hidden');
        modal.style.display = 'none';
        iframe.src = 'about:blank';
        unlockBackgroundScroll();
    }, 300);
}

function toggleExecutionMaximize() {
    const modal = document.getElementById('executionModal');
    const modalContent = document.getElementById('executionModalContent');
    const modalHeader = document.getElementById('executionModalHeader');
    const maxIcon = document.getElementById('execMaximizeIcon');
    const restoreIcon = document.getElementById('execRestoreIcon');
    const isMaximized = modalContent.classList.contains('h-screen');
    const isHeaderHidden = modalHeader?.classList.contains('hidden');

    if (!isMaximized) {
        modal.classList.remove('p-4');
        modal.classList.add('p-0');
        modalContent.classList.remove('max-w-7xl', 'h-[95vh]', 'h-[98vh]', 'rounded-2xl');
        modalContent.classList.add('w-full', 'h-screen', 'max-w-none', 'max-h-none', 'rounded-none');
        if (maxIcon && restoreIcon) {
            maxIcon.classList.add('hidden');
            restoreIcon.classList.remove('hidden');
        }
    } else {
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

