CKEDITOR.plugins.add('imageid', {
    requires: 'widget',
    icons: 'handle',

    init: function (editor) {

        CKEDITOR.dialog.add('imageid', this.path + 'dialogs/imageid.js');

        editor.widgets.add('imageid', {

            allowedContent: 'div(!imageid); img(!imageid-img); div(!imageid-caption)',

            button: 'Bilde',

            editables: {
                caption: {
                    selector: '.imageid-caption'
                }
            },

            template:
                '<div class="imageid">' +
                    '<img class="imageid-img" src="" />' +
                    '<div class="imageid-caption"><p>Bildetekst</p></div>' +
                '</div>',

            upcast: function (element) {
                return element.name == 'div' && element.hasClass('imageid');
            }

        });
    }
});