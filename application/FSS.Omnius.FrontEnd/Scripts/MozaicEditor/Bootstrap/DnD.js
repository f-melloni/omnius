﻿MBE.DnD = {
    
    currentElement: null,
    placeholder: null,
    onDOMUpdate: [],
    onDragEnter: [],
    onDragLeave: [],
    onDragEnd: [],
    onDrop: [],
    onBeforeDrop: [],

    isNavNodeDragging: false,
    isUICDragging: false,
    domNeedUpdate: false,

    init: function()
    {
        var self = MBE.DnD;

        self.placeholder = $('#drop-placeholder');
        self.placeholder.hide();

        $('ul.category > li > ul > li').attr('draggable', true);
        $('ul.category > li > ul > li').each(function () {
            $(this).data('type', $(this).parent().data('type'));
        });

        $(document)
            .on('dragstart', 'ul.category li[draggable=true]', self._componentDragStart)
            .on('dragstart', '.tree-nav .node-handle[draggable=true]', self._navNodeDragStart)
            .on('dragover', '.tree-nav .node:not(.dragged)', self._navNodeDragOver)
            .on('drop', '.tree-nav .node:not(.dragged)', self._navNodeDrop)
            .on('dragend dragstop', self._dragEnd)
        ;

        $(MBE.workspaceDoc)
            .on('dragover', "body, [data-uic]:not(.dragged)", self._dragOver)
            .on('dragenter', "body, [data-uic]", self._dragEnter)
            .on('dragleave', "body, [data-uic]", self._dragLeave)
            .on('dragstart', '.mbe-drag-handle[draggable=true]', self._uicDragStart)
            .on('drop', "body, [data-uic]", self._workSpaceDrop)
            .on('dragend dragstop', self._dragEnd)
        ;
    },

    getUIC: function() 
    {
        var self = MBE.DnD;
        var item = $(self.currentElement)
        
        if (item.hasClass('node')) {
            return item.find('> .node-handle b').data('targetuic');
        }
        else if (!item.is('[data-uic]')) {
            return self.createUIC(item);
        }
        
        return item;
    },

    createUIC: function (item)
    {
        var type = item.data('type');
        var template = item.data('template');

        var elm = $(MBE.types[type].templates[template]);
        if (!elm.data('uic')) {
            elm.attr('data-uic', type + "|" + template);
        }

        return elm;
    },

    createGhost: function()
    {
        var ghost = $('<span class="drag-ghost" style="position:absolute; left: -100000px; top: -100000px"></span>');
        ghost.appendTo('body');

        return ghost[0];
    },

    /*******************************************************/
    /* START                                               */
    /*******************************************************/
    _componentDragStart: function(event)
    {
        var e = event.originalEvent; 
        
        MBE.DnD.currentElement = this;

        var ghost = MBE.DnD.createGhost();
        $('body').addClass('dragging');

        e.dataTransfer.setData('text/plain', '...');
        e.dataTransfer.effectAllowed = 'all';
        e.dataTransfer.setDragImage(ghost, -12, -12);
    }, 

    _navNodeDragStart: function(event)
    {
        var e = event.originalEvent;

        MBE.DnD.currentElement = $(this).parent();
        MBE.DnD.currentElement.addClass('dragged');
        MBE.DnD.isNavNodeDragging = true;

        var ghost = MBE.DnD.createGhost();
        $('body').addClass('dragging');

        e.dataTransfer.setData('text/plain', '...');
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setDragImage(ghost, -12, -12);
    },

    _uicDragStart: function (event) {
        var e = event.originalEvent;

        MBE.DnD.currentElement = $('.mbe-active', MBE.workspace);
        MBE.DnD.currentElement.addClass('dragged');
        MBE.DnD.isUICDragging = true;

        var ghost = MBE.DnD.createGhost();
        $('body').addClass('dragging');

        e.dataTransfer.setData('text/plain', '...');
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setDragImage(ghost, -12, -12);
    },

    /*******************************************************/
    /* OVER                                                */
    /*******************************************************/
    _dragOver: function(event)
    {
        event.preventDefault();
        var target = $(this);
        var childs = target.find(' > *');

        if (target.is('.dragged') || target.parents('.dragged').length) {
            event.originalEvent.dataTransfer.effectAllowed = 'none';
            MBE.DnD.placeholder.hide();
            return false;
        }

        if(!childs.length) { // cíl neobsahuje žádné další prvky - vložíme na začátek
            target.append(MBE.DnD.placeholder);
        }
        else { // Vypočítáme pozici
            var y = event.pageY;
        
            for (var i = 0; i < childs.length; i++) {
                var top = childs.eq(i).offset().top;
        
                if (top >= y - 5 && top <= y + 5) { // Umístíme placeholder před element
                    childs.eq(i).before(MBE.DnD.placeholder);
                    break;
                }
                else { // Umístíme placeholder za element
                    childs.eq(i).after(MBE.DnD.placeholder);
                }
            }
        }
        MBE.DnD.placeholder.show();

        return false;
    },

    _navNodeDragOver: function(event)
    {
        event.preventDefault();
        
        var target = $(this);
        
        var y = event.pageY;
        var top = target.offset().top;
        var height = target.outerHeight();

        $('b.drop-target').removeClass('drop-target');

        if (top >= y - 3 && top <= y + 3 && !target.hasClass('root')) {
            target.before(MBE.DnD.placeholder);
        }
        else if(top + height >= y - 3 && top + height <= y + 3 && !target.hasClass('root')) {
            target.after(MBE.DnD.placeholder);
        }
        else {
            target.find('> .sub-tree').append(MBE.DnD.placeholder);
            target.find('> .node-handle b').addClass('drop-target');
        }
        
        return false;
    },
     
    /*******************************************************/
    /* DROP                                                */
    /*******************************************************/
    _workSpaceDrop: function(event)
    {
        event.stopImmediatePropagation();
        event.preventDefault();
            
        var self = MBE.DnD;
        var uic = self.getUIC();

        self.callListeners('onBeforeDrop', uic[0], [self.placeholder.parent()]);
        self.placeholder.after(uic);
        self.callListeners('onDrop', uic[0], [self.placeholder.parent()]);
        self.domNeedUpdate = true;
    },

    _navNodeDrop: function(event)
    {
        event.stopImmediatePropagation();
        event.preventDefault();

        var self = MBE.DnD;
        var target = self.placeholder;
        
        var item = $(self.currentElement);
        var uic;

        if (item.is('.node')) {
            uic = item.find('> .node-handle b').data('targetuic');
        }
        else if (!item.is('[data-uic]')) {
            uic = self.createUIC(item);
        }
        else {
            uic = item;
        }

        self.callListeners('onBeforeDrop', $(uic)[0], [$(uic).parent()]);

        if (target.prev('.node').length) {
            $(target.prev('.node').find('> .node-handle b').data('targetuic')).after(uic);
        }
        else if (target.next('.node').length) {
            $(target.next('.node').find('> .node-handle b').data('targetuic')).before(uic);
        }
        else {
            $(target.parents('.node').eq(0).find('> .node-handle b').data('targetuic')).append(uic);
        }

        self.callListeners('onDrop', $(uic)[0], [$(uic).parent()]);
        self.domNeedUpdate = true;
    },

    /*******************************************************/
    /* COMMON                                              */
    /*******************************************************/
    _dragEnter: function(event)
    {
        event.stopImmediatePropagation();
        if ($(this).is('.dragged') || $(this).parents('.dragged').length) {
            return;
        }
        $(this).addClass('drag-over');
        MBE.DnD.callListeners('onDragEnter', this);
    },

    _dragLeave: function(event)
    {
        event.stopImmediatePropagation();
        if ($(this).is('.dragged') || $(this).parents('.dragged').length) {
            return;
        }
        $(this).removeClass('drag-over');
        MBE.DnD.callListeners('onDragLeave', this);
    },

    _dragEnd: function(event)
    {
        $('body').removeClass('dragging');
        $('.drag-over').removeClass('drag-over');
        $('.drag-over', MBE.workspace).removeClass('drag-over');
        $('.drag-ghost').remove();

        $('#mozaicPageWorkspace').after(MBE.DnD.placeholder);
        MBE.DnD.placeholder.hide();

        if (MBE.DnD.isNavNodeDragging) {
            MBE.DnD.currentElement.removeClass('dragged');
            MBE.DnD.isNavNodeDragging = false;
        }
        if (MBE.DnD.isUICDragging) {
            MBE.DnD.currentElement.removeClass('dragged');
            MBE.DnD.isUICDragging = false;
        }

        MBE.DnD.currentElement = null;
        MBE.DnD.callListeners('onDragEnd', this);

        if (MBE.DnD.domNeedUpdate) {
            MBE.DnD.updateDOM();
        }
    },

    updateDOM: function()
    {
        $('[data-uic]', MBE.workspace).removeClass('empty-element');
        $('[data-uic]:not(input, select, hr, img, .caret, li.divider, .fa, .glyphicon):empty', MBE.workspace).addClass('empty-element');

        MBE.workspace.find('*:not(iframe, script, style, svg, svg *, option)').contents().filter(function () {
            return this.nodeType == Node.TEXT_NODE && !$(this).parent().hasClass('mbe-text-node') && !$(this).parents('[data-uic="misc|custom-code"]').length;
        }).wrap('<span class="mbe-text-node" />');

        MBE.workspace.find('.has-feedback').each(function() {
            if (!$(this).find('.form-control-feedback').length) {
                $(this).removeClass('has-feedback');
            }
        })

        MBE.DnD.callListeners('onDOMUpdate', MBE.workspace[0]);

        MBE.DnD.domNeedUpdate = false;
    },

    callListeners: function(eventType, context, params)
    {
        for (var i = 0; i < MBE.DnD[eventType].length; i++) {
            var f = MBE.DnD[eventType][i];
            f.apply(context, params ? params : []);
        }
    }
}

MBE.onInit.push(MBE.DnD.init);