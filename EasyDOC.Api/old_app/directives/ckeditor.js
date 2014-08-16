angular.module('app').directive('ckEditor', function () {
    return {
        require: '?ngModel',
        link: function (scope, element, attr, ngModel) {

            var instanceReady = false;

            var ck = CKEDITOR.replace(element[0], {
                height: 600,
                width: 700,
                extraPlugins: 'font,find,selectall,colorbutton,justify,table,tabletools,tableresize,showborders,zoom',
                removePlugins: 'bidi,dialogadvtab,div,flash,forms,horizontalrule,iframe,liststyle,pagebreak,showborders,stylescombo,templates',
                toolbar: [
                    ['Source', 'AjaxSave', '-', 'Undo', 'Redo'],
                    ['TextColor'],
                    ['JustifyLeft', 'JustifyCenter', 'JustifyRight'],
                    ['Bold', 'Italic', 'Underline', '-', 'RemoveFormat'],
                    ['BulletedList'],
                    ['Indent', 'Outdent'],
                    ['Table', 'Image'],
                    ['Find', 'Replace'],
                    ['Link', 'Unlink', 'SpecialChar'],
                    ['Maximize']
                ],
                on: {
                    instanceReady: function () {
                        if (scope.loaded) {
                            instanceReady = true;
                            ck.setData(ngModel.$viewValue);
                        }
                    },
                    key: updateModel,
                    change: updateModel
                }
            });

            if (!ngModel) return;

            function updateModel() {
                if (instanceReady && scope.loaded) {

                    setTimeout(function () {
                        scope.$apply(function () {
                            ngModel.$setViewValue(ck.getData());
                        });
                    }, 0);
                }
            }

            ngModel.$render = function (value) {
                if (instanceReady) {
                    ck.setData(ngModel.$viewValue);
                }
            };
        }
    };
});