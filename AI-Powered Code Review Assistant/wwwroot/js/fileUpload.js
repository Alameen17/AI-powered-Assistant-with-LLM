// File Upload JavaScript Helper for AI-Powered Code Review Assistant
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

// Handle file drop for drag and drop functionality
window.handleFileDrop = function (dropEvent) {
    console.log('Files dropped:', dropEvent);
    // File drop handling is done through the file input component
};
