// File Upload JavaScript
window.fileUploadHelper = {
    componentInstance: null,

    setComponentInstance: function(instance) {
        this.componentInstance = instance;
    },

    initializeDropZone: function (dropZoneId, inputId) {
        const dropZone = document.getElementById(dropZoneId);
        const fileInput = document.getElementById(inputId);

        if (!dropZone || !fileInput) return;

        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, preventDefaults, false);
            document.body.addEventListener(eventName, preventDefaults, false);
        });

        function preventDefaults(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        // Highlight drop zone when item is dragged over it
        ['dragenter', 'dragover'].forEach(eventName => {
            dropZone.addEventListener(eventName, highlight, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, unhighlight, false);
        });

        function highlight(e) {
            dropZone.classList.add('dragover');
        }

        function unhighlight(e) {
            dropZone.classList.remove('dragover');
        }

        // Handle dropped files
        dropZone.addEventListener('drop', handleDrop, false);

        function handleDrop(e) {
            const dt = e.dataTransfer;
            const files = dt.files;
            handleFiles(files);
        }

        // Handle file selection
        fileInput.addEventListener('change', function(e) {
            handleFiles(e.target.files);
        });

        function handleFiles(files) {
            [...files].forEach(uploadFile);
        }

        function uploadFile(file) {
            // Check file type
            const allowedExtensions = ['.cs', '.js', '.ts', '.py', '.java', '.cpp', '.go', '.rs', '.php', '.rb', '.c', '.h', '.jsx', '.tsx', '.vue'];
            const fileExtension = '.' + file.name.split('.').pop().toLowerCase();

            if (!allowedExtensions.includes(fileExtension)) {
                alert(`File type ${fileExtension} is not supported. Please upload code files only.`);
                return;
            }

            // Check file size (5MB limit)
            if (file.size > 5 * 1024 * 1024) {
                alert('File size must be less than 5MB');
                return;
            }

            // Read file content
            const reader = new FileReader();
            reader.onload = function(e) {
                const content = e.target.result;
                if (window.fileUploadHelper.componentInstance) {
                    window.fileUploadHelper.componentInstance.invokeMethodAsync('AddUploadedFile', file.name, content);
                }
            };
            reader.readAsText(file);
        }

        // Click to browse
        dropZone.addEventListener('click', function() {
            fileInput.click();
        });
    },

    selectDirectory: async function() {
        try {
            if ('showDirectoryPicker' in window) {
                const dirHandle = await window.showDirectoryPicker();
                return await this.processDirectory(dirHandle);
            } else {
                alert('Directory selection is not supported in this browser. Please use the manual path input instead.');
                return null;
            }
        } catch (err) {
            console.error('Directory selection cancelled or failed:', err);
            return null;
        }
    },

    processDirectory: async function(dirHandle, path = '') {
        const files = [];

        for await (const entry of dirHandle.values()) {
            const fullPath = path ? `${path}/${entry.name}` : entry.name;

            if (entry.kind === 'file') {
                const allowedExtensions = ['.cs', '.js', '.ts', '.py', '.java', '.cpp', '.go', '.rs', '.php', '.rb', '.c', '.h', '.jsx', '.tsx', '.vue'];
                const fileExtension = '.' + entry.name.split('.').pop().toLowerCase();

                if (allowedExtensions.includes(fileExtension)) {
                    try {
                        const file = await entry.getFile();
                        if (file.size <= 5 * 1024 * 1024) { // 5MB limit
                            const content = await file.text();
                            files.push({
                                name: fullPath,
                                content: content
                            });
                        }
                    } catch (err) {
                        console.warn(`Could not read file ${fullPath}:`, err);
                    }
                }
            } else if (entry.kind === 'directory' && !entry.name.startsWith('.')) {
                const subFiles = await this.processDirectory(entry, fullPath);
                files.push(...subFiles);
            }
        }

        return files;
    },

    browseForDirectory: async function() {
        try {
            if ('showDirectoryPicker' in window) {
                const dirHandle = await window.showDirectoryPicker();
                return dirHandle.name; // Return the directory name/path
            } else {
                alert('Directory browsing is not supported in this browser. Please enter the path manually.');
                return null;
            }
        } catch (err) {
            console.log('Directory selection cancelled or failed:', err);
            return null;
        }
    }
};

// Simple function to trigger file input
window.triggerFileInput = function(element) {
    if (element) {
        element.click();
    }
};

// Function to download file
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

// UI Responsiveness Helper Functions
// Enhanced file upload helper with directory browsing
window.fileUploadHelper = {
    componentInstance: null,

    setComponentInstance: function(instance) {
        this.componentInstance = instance;
    },

    initializeDropZone: function (dropZoneId, inputId) {
        const dropZone = document.getElementById(dropZoneId);
        const fileInput = document.getElementById(inputId);

        if (!dropZone || !fileInput) return;

        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, preventDefaults, false);
            document.body.addEventListener(eventName, preventDefaults, false);
        });

        function preventDefaults(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        // Highlight drop zone when item is dragged over it
        ['dragenter', 'dragover'].forEach(eventName => {
            dropZone.addEventListener(eventName, highlight, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, unhighlight, false);
        });

        function highlight(e) {
            dropZone.classList.add('dragover');
        }

        function unhighlight(e) {
            dropZone.classList.remove('dragover');
        }

        // Handle dropped files
        dropZone.addEventListener('drop', handleDrop, false);

        function handleDrop(e) {
            const dt = e.dataTransfer;
            const files = dt.files;
            handleFiles(files);
        }

        // Handle file selection
        fileInput.addEventListener('change', function(e) {
            handleFiles(e.target.files);
        });

        function handleFiles(files) {
            [...files].forEach(uploadFile);
        }

        function uploadFile(file) {
            // Check file type
            const allowedExtensions = ['.cs', '.js', '.ts', '.py', '.java', '.cpp', '.go', '.rs', '.php', '.rb', '.c', '.h', '.jsx', '.tsx', '.vue'];
            const fileExtension = '.' + file.name.split('.').pop().toLowerCase();

            if (!allowedExtensions.includes(fileExtension)) {
                alert(`File type ${fileExtension} is not supported. Please upload code files only.`);
                return;
            }

            // Check file size (5MB limit)
            if (file.size > 5 * 1024 * 1024) {
                alert('File size must be less than 5MB');
                return;
            }

            // Read file content
            const reader = new FileReader();
            reader.onload = function(e) {
                const content = e.target.result;
                if (window.fileUploadHelper.componentInstance) {
                    window.fileUploadHelper.componentInstance.invokeMethodAsync('AddUploadedFile', file.name, content);
                }
            };
            reader.readAsText(file);
        }

        // Click to browse
        dropZone.addEventListener('click', function() {
            fileInput.click();
        });
    },

    selectDirectory: async function() {
        try {
            if ('showDirectoryPicker' in window) {
                const dirHandle = await window.showDirectoryPicker();
                return await this.processDirectory(dirHandle);
            } else {
                alert('Directory selection is not supported in this browser. Please use the manual path input instead.');
                return null;
            }
        } catch (err) {
            console.error('Directory selection cancelled or failed:', err);
            return null;
        }
    },

    processDirectory: async function(dirHandle, path = '') {
        const files = [];

        for await (const entry of dirHandle.values()) {
            const fullPath = path ? `${path}/${entry.name}` : entry.name;

            if (entry.kind === 'file') {
                const allowedExtensions = ['.cs', '.js', '.ts', '.py', '.java', '.cpp', '.go', '.rs', '.php', '.rb', '.c', '.h', '.jsx', '.tsx', '.vue'];
                const fileExtension = '.' + entry.name.split('.').pop().toLowerCase();

                if (allowedExtensions.includes(fileExtension)) {
                    try {
                        const file = await entry.getFile();
                        if (file.size <= 5 * 1024 * 1024) { // 5MB limit
                            const content = await file.text();
                            files.push({
                                name: fullPath,
                                content: content
                            });
                        }
                    } catch (err) {
                        console.warn(`Could not read file ${fullPath}:`, err);
                    }
                }
            } else if (entry.kind === 'directory' && !entry.name.startsWith('.')) {
                const subFiles = await this.processDirectory(entry, fullPath);
                files.push(...subFiles);
            }
        }

        return files;
    },

    browseForDirectory: function () {
        return new Promise((resolve) => {
            const input = document.createElement('input');
            input.type = 'file';
            input.webkitdirectory = true;
            input.directory = true;

            input.onchange = (e) => {
                if (e.target.files.length > 0) {
                    // Get the directory path from the first file
                    const path = e.target.files[0].webkitRelativePath.split('/')[0];
                    resolve(path);
                } else {
                    resolve(null);
                }
            };

            input.click();
        });
    }
};

// Trigger file input click
window.triggerFileInput = function (element) {
    if (element) {
        element.value = null; // Reset to allow selecting the same file
        element.click();
    }
};

// Handle file drop
window.handleFileDrop = function (dropEvent) {
    // This would need to be implemented with proper JavaScript interop
    // to handle the dropped files and pass them back to Blazor
    console.log('Files dropped:', dropEvent);
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

// Initialize tooltips (if using Bootstrap)
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

// File size formatter
window.formatFileSize = function (bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
};

// Debounce function for performance
window.debounce = function (func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

// Animate numbers (for stats)
window.animateNumber = function (element, start, end, duration) {
    const range = end - start;
    const increment = range / (duration / 16); // 60 FPS
    let current = start;

    const timer = setInterval(() => {
        current += increment;
        if (current >= end) {
            current = end;
            clearInterval(timer);
        }
        element.textContent = Math.floor(current);
    }, 16);
};

// Initialize animations when elements come into view
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('animated');

            // Animate numbers if it's a stat element
            if (entry.target.classList.contains('stat-number')) {
                const endValue = parseInt(entry.target.textContent);
                animateNumber(entry.target, 0, endValue, 1000);
            }
        }
    });
}, observerOptions);

// Observe elements for animation
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.fade-in, .fade-in-up, .fade-in-down, .stat-number').forEach(el => {
        observer.observe(el);
    });
});

// Export functions for Blazor
window.codeReviewHelpers = {
    formatFileSize: window.formatFileSize,
    debounce: window.debounce,
    animateNumber: window.animateNumber,
    showKeyboardShortcuts: window.showKeyboardShortcuts,
    setupKeyboardShortcuts: window.setupKeyboardShortcuts
};
