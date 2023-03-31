$(function () {
    //CKEditor
    CKEDITOR.replace('isiberita');
    CKEDITOR.config.height = 150;

    CKEDITOR.editorConfig = function (config) {
        config.removePlugins = 'toolbar';
    };
});