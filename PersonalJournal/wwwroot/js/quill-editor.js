let quill;

window.initQuill = function(elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.error('Element not found:', elementId);
        return;
    }

    quill = new Quill('#' + elementId, {
        theme: 'snow',
        placeholder: 'Write about your day...',
        modules: {
            toolbar: [
                [{ 'header': [1, 2, 3, false] }],
                ['bold', 'italic', 'underline', 'strike'],
                [{ 'list': 'ordered' }, { 'list': 'bullet' }],
                ['link'],
                ['clean']
            ]
        }
    });
}

window.getQuillContent = function() {
    if (!quill) return '';
    return quill.root.innerHTML;
}

window.setQuillContent = function(html) {
    if (!quill) return;
    quill.root.innerHTML = html || '';
}