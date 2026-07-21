window.select2Helper = {

    init: function (id, data, selected) {

        if (!window.jQuery) {
            console.error("jQuery not loaded");
            return;
        }

        if (!$.fn.select2) {
            console.error("Select2 not loaded");
            return;
        }

        const el = $('#' + id);

        if (!el.length) {
            console.error("Element not found:", id);
            return;
        }

        if (el.hasClass("select2-hidden-accessible")) {
            el.select2('destroy');
        }

        el.empty();

        data.forEach(x => {
            el.append(new Option(x.text, x.id));
        });

        el.select2({
            placeholder: "Sélectionner",
            width: '100%',
            allowClear: true
        });

        if (selected && selected.length > 0) {
            el.val(selected).trigger('change');
        } else {
            el.val(null).trigger('change');
        }
    },
    initAjax: function (elementId, url) {
        $('#' + elementId).select2({
            placeholder: "-- Sélectionner un responsable --",
            allowClear: true,
            minimumInputLength: 3,
            ajax: {
                url: url,
                dataType: 'json',
                delay: 300,
                data: function (params) {
                    return {
                        search: params.term
                    };
                },
                processResults: function (response) {
                    return {
                        results: response.data.map(u => ({
                            id: u.id,
                            text: u.text
                        }))
                    };
                },
                cache: true
            }
        });
    },
    setValue: function (id, value) {
        const el = $('#' + id);

        if (value) {
            el.val(value).trigger('change');
        } else {
            el.val(null).trigger('change');
        }
    },
    getValue: function (elementId) {
        return $('#' + elementId).val();
    },
    getSelected: function (id) {
        return $('#' + id).val();
    },
    setSelectedAjax : function (elementId, id, text) {
        const select = $('#' + elementId);

        const option = new Option(text, id, true, true);
        select.append(option).trigger('change');
    },
    setValueAjax: function (id, value, text) {
        const el = $('#' + id);

        if (!value) {
            el.val(null).trigger('change');
            return;
        }

        // Create the option if it doesn't exist
        const option = new Option(text, value, true, true);
        el.append(option).trigger('change');
    }
};