(function() {

    angular.module('app').directive('ckEditor', function() {
        return {
            require: '?ngModel',
            link: function(scope, element, attr, ngModel) {

                var instanceReady = false;
                var scopeReady = false;

                var ck = CKEDITOR.replace(element[0], {
                    height: 600,
                    width: 700,
                    lang: 'no',
                    extraPlugins: 'font,find,selectall,colorbutton,imageid,justify,pagebreak,table,tabletools,tableresize,showborders,zoom',
                    removePlugins: 'bidi,dialogadvtab,div,flash,forms,horizontalrule,iframe,liststyle,showborders,stylescombo,templates',
                    toolbar: [
                        ['Source', '-', 'Undo', 'Redo'],
                        ['TextColor'],
                        ['JustifyLeft', 'JustifyCenter', 'JustifyRight'],
                        ['Bold', 'Italic', 'Underline', '-', 'RemoveFormat'],
                        ['BulletedList'],
                        ['Indent', 'Outdent'],
                        ['PageBreak', 'Table', 'Image', 'Imageid'],
                        ['Find', 'Replace'],
                        ['Link', 'Unlink', 'SpecialChar'],
                        ['Maximize']
                    ],
                    on: {
                        instanceReady: function() {
                            instanceReady = true;
                            if (scopeReady) {
                                ck.setData(ngModel.$viewValue);
                            }
                        },
                        change: updateModel
                    }
                });

                function updateModel() {
                    if (instanceReady && scope.loaded) {

                        setTimeout(function() {
                            scope.$apply(function() {
                                ngModel.$setViewValue(ck.getData());
                            });
                        }, 0);
                    }
                }

                ngModel.$render = function(value) {
                    scopeReady = true;
                    if (instanceReady) {
                        ck.setData(ngModel.$viewValue);
                    }
                };
            }
        };
    });
})();