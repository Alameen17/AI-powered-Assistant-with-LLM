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