const useDefaults = document.getElementById('use_defaults');
if (useDefaults) {
    useDefaults.addEventListener('change', function (e) {
        const settings = document.getElementById('lang_settings');
        if (!settings) return;
        settings.classList.toggle('hidden', e.target.checked);
    });
}

const testMode = document.getElementById('test_mode');
if (testMode) {
    testMode.addEventListener('change', function (e) {
        const form = document.getElementById('loginForm');
        const workDirInput = document.getElementById('work_dir');
        if (!form) return;

        if (e.target.checked) {
            form.action = '/test/login';
            if (workDirInput) workDirInput.value = 'Test Mode (app_test)';
            console.log('Switched to Test Mode: /test/login');
        } else {
            form.action = '/Account/Login';
            if (workDirInput) workDirInput.value = '';
            console.log('Switched to Normal Mode: /Account/Login');
        }
    });
}

document.getElementById('loginForm')?.addEventListener('submit', function () {
    const loadingModal = document.getElementById('loadingModal');
    loadingModal?.classList.remove('hidden');

    const loginBtn = document.getElementById('loginBtn');
    if (loginBtn) {
        loginBtn.disabled = true;
        loginBtn.innerHTML = '<svg class="w-5 h-5 animate-spin inline mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg>登入中...';
    }

    const messages = [
        '正在驗證帳號密碼...',
        '正在建立資料庫連線...',
        '正在載入使用者資訊...',
        '正在進入系統...'
    ];

    let messageIndex = 0;
    const loadingMessage = document.getElementById('loadingMessage');
    if (!loadingMessage) return;

    setInterval(function () {
        if (messageIndex >= messages.length) return;
        loadingMessage.innerHTML =
            '<svg class="w-5 h-5 inline mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>' + messages[messageIndex];
        messageIndex++;
    }, 1200);
});

