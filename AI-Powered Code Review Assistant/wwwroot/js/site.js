// AI-Powered Code Review Assistant - Main JavaScript File

// Download file function
window.downloadFile = function(filename, content, contentType) {
    const a = document.createElement('a');
    const blob = new Blob([content], { type: contentType });
    const url = window.URL.createObjectURL(blob);

    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();

    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
};

// Setup keyboard shortcuts
window.setupKeyboardShortcuts = function () {
    document.addEventListener('keydown', function (e) {
        // Ctrl/Cmd + O to open file browser
        if ((e.ctrlKey || e.metaKey) && e.key === 'o') {
            e.preventDefault();
            const browseBtn = document.querySelector('.input-addon-btn');
            if (browseBtn && !browseBtn.disabled) {
                browseBtn.click();
            }
        }

        // Ctrl/Cmd + Enter to start analysis
        if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
            e.preventDefault();
            const submitBtn = document.querySelector('.custom-btn:not(:disabled)');
            if (submitBtn) {
                submitBtn.click();
            }
        }

        // Ctrl/Cmd + N for new analysis
        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            window.location.href = '/review';
        }

        // Escape to clear errors
        if (e.key === 'Escape') {
            const closeBtn = document.querySelector('.btn-close');
            if (closeBtn) {
                closeBtn.click();
            }
        }
    });
};

// Show keyboard shortcuts modal
window.showKeyboardShortcuts = function () {
    const shortcuts = [
        { key: 'Ctrl/Cmd + O', description: 'Open file/directory browser' },
        { key: 'Ctrl/Cmd + Enter', description: 'Start analysis' },
        { key: 'Ctrl/Cmd + N', description: 'New analysis' },
        { key: 'Escape', description: 'Close error messages' },
        { key: 'Tab', description: 'Navigate between fields' }
    ];

    // Create modal
    const modal = document.createElement('div');
    modal.className = 'keyboard-shortcuts-modal';
    modal.innerHTML = `
        <div class="modal-backdrop" onclick="this.parentElement.remove()"></div>
        <div class="modal-content">
            <h3>Keyboard Shortcuts</h3>
            <div class="shortcuts-list">
                ${shortcuts.map(s => `
                    <div class="shortcut-item">
                        <kbd>${s.key}</kbd>
                        <span>${s.description}</span>
                    </div>
                `).join('')}
            </div>
            <button class="btn btn-primary" onclick="this.closest('.keyboard-shortcuts-modal').remove()">
                Close
            </button>
        </div>
    `;

    document.body.appendChild(modal);
};

// Add custom modal styles
const style = document.createElement('style');
style.textContent = `
    .keyboard-shortcuts-modal {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        z-index: 9999;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .modal-backdrop {
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.5);
        backdrop-filter: blur(5px);
    }

    .modal-content {
        position: relative;
        background: white;
        border-radius: 15px;
        padding: 2rem;
        max-width: 500px;
        width: 90%;
        max-height: 80vh;
        overflow-y: auto;
        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
        animation: modalSlideIn 0.3s ease;
    }

    @keyframes modalSlideIn {
        from {
            opacity: 0;
            transform: translateY(-20px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    .modal-content h3 {
        margin-bottom: 1.5rem;
        color: #2d3748;
        font-size: 1.5rem;
    }

    .shortcuts-list {
        margin-bottom: 1.5rem;
    }

    .shortcut-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0.75rem;
        border-bottom: 1px solid #e2e8f0;
    }

    .shortcut-item:last-child {
        border-bottom: none;
    }

    .shortcut-item kbd {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        padding: 0.25rem 0.75rem;
        border-radius: 6px;
        font-family: 'Courier New', monospace;
        font-size: 0.875rem;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .shortcut-item span {
        color: #4a5568;
        margin-left: 1rem;
    }
`;
document.head.appendChild(style);

// Initialize on document ready
document.addEventListener('DOMContentLoaded', function () {
    // Initialize any tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    if (typeof bootstrap !== 'undefined') {
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Add smooth scroll behavior
    document.documentElement.style.scrollBehavior = 'smooth';
});
