/*
این فایل منو را بصورت صحیح نمایش می دهد
بصورتیکه اگر منو خیلی پایین باز شد و از صفحه خارج شد، 
موقیعت آنرا طوری تصحیح کند که منو بصورت کامل در صفحه دیده شود.
توجه شود که این موضوع فقط بر روی پایین "کانتینر" اعمال شده است.
یعنی اگر منو از بالا در دید نباشد نیاز است که اسکریپت تصحیح شود
*/

if ($.fn.scrollParent == null)
    throw "You need scrollparent.js for telerik.menu : https://github.com/slindberg/jquery-scrollparent/blob/master/jquery.scrollparent.js";

(function ($) {
    var $t = $.telerik;

    $t.scripts.push("telerik.menu.js");

    $t.menu = function (element, options) {
        this.element = element;
        this.nextItemZIndex = 100;

        $.extend(this, options);

        $('.t-item:not(.t-state-disabled)', element)
            .live('mouseenter', $t.delegate(this, this.mouseenter))
            .live('mouseleave', $t.delegate(this, this.mouseleave))
            .live('click', $t.delegate(this, this.click));

        $('.t-item:not(.t-state-disabled) > .t-link', element)
            .live('mouseenter', $t.hover)
            .live('mouseleave', $t.leave);

        $('.t-item.t-state-disabled', element)
            .live('click', function () { return false; });

        $(document).click($t.delegate(this, this.documentClick));

        $t.bind(this, {
            select: this.onSelect,
            open: this.onOpen,
            close: this.onClose,
            load: this.onLoad
        });
    }

    function getEffectOptions(item) {
        var parent = item.parent();
        return {
            direction: parent.hasClass('t-menu') ? parent.hasClass('t-menu-vertical') ? 'right' : 'bottom' : 'right'
        };
    };

    function contains(parent, child) {
        try {
            return $.contains(parent, child);
        } catch (e) {
            return false;
        }
    }

    $t.menu.prototype = {

        toggle: function (li, enable) {
            $(li).each(function () {
                $(this)
                    .toggleClass('t-state-default', enable)
                    .toggleClass('t-state-disabled', !enable);
            });
        },

        enable: function (li) {
            this.toggle(li, true);
        },

        disable: function (li) {
            this.toggle(li, false);
        },

        open: function ($li) {
            var menu = this;
            var isTopOpen = $(menu.element).attr('g-orientation') == 'top';//TODO : we don not manage out of viewport in top opening.

            $($li).each(function () {
                var $item = $(this);

                clearTimeout($item.data('timer'));

                $item.data('timer', setTimeout(function () {
                    var $ul = $item.find('.t-group:first');
                    if ($ul.length) {

                        if (!isTopOpen) {
                            //1: make the menu height calculatable before showing (moravej) http://stackoverflow.com/a/2345813/790811
                            var isAnimationPanelCreated = $ul.parent().hasClass('t-animation-container');
                            var container = isAnimationPanelCreated ? $ul.parent() : $ul;// at first show, there is no animation container.

                            if (old = container.data('old-bottom')) {
                                container.css('bottom', old);//return to main value to be able to calculate position again.
                                container.css('top', container.data('old-top'));
                            }

                            var mainStyle = container.attr('style');
                            container.css({
                                position: 'absolute',
                                visibility: 'hidden',
                                display: 'block'
                            });

                            //2nd: check if out of viewport
                            viewport = container.scrollParent();//returns first scrollable parent
                            viewport = viewport == null || viewport.length == 0 ? null : viewport[0];
                            viewport = viewport == null || viewport == document ? document.body : viewport;

                            viewportBottom = Math.min(viewport.getBoundingClientRect().bottom, document.body.getBoundingClientRect().bottom);
                            containerRec = container[0].getBoundingClientRect();
                            if (containerRec.height == 0) containerRec = $ul[0].getBoundingClientRect();//at first request!



                            //3: reset style to original.
                            container.attr('style', mainStyle ? mainStyle : '');

                            //4: correct it if we are out of viewport and animation panel presents.
                            var effects = getEffectOptions($item);
                            var menuButtonBottom = $item[0].getBoundingClientRect().bottom;
                            var availableBottom = -(viewportBottom - menuButtonBottom) + 20;//20 is for scrollbar if any

                            if (containerRec.bottom > viewportBottom && isAnimationPanelCreated) {
                                container.data('old-bottom', container.css('bottom'));//we need it to support resizing of page (Which causes the menu to be show in previous mode again)
                                container.data('old-top', container.css('top'));
                                container.css('bottom', availableBottom);
                                container.css('top', 'auto');//it forces bottom value. we need it for left/right sided menus
                            }


                            //5: open menu (and it creates animation container if not any)
                            $t.fx.play(menu.effects, $ul, effects);

                            //6: correct if we are out of viewport and animation panel was not present at previous step.
                            if (containerRec.bottom > viewportBottom && !isAnimationPanelCreated) {
                                container = container.parent();

                                container.data('old-bottom', container.css('bottom'));//we need it to support resizing of page (Which causes the menu to be show in previous mode again)
                                container.data('old-top', container.css('top'));
                                container.css('bottom', availableBottom);
                                container.css('top', 'auto');//it forces bottom value. we need it for left/right sided menus
                            }
                        }
                        else
                            $t.fx.play(menu.effects, $ul, getEffectOptions($item));


                        $item.css('z-index', menu.nextItemZIndex++);
                    }
                }, 100));
            });
        },

        close: function ($li) {
            var menu = this;

            $($li).each(function (index, item) {
                var $item = $(item);

                clearTimeout($item.data('timer'));

                $item.data('timer', setTimeout(function () {
                    var $ul = $item.find('.t-group:first');
                    if ($ul.length) {
                        $t.fx.rewind(menu.effects, $ul, getEffectOptions($item), function () {
                            $item.css('zIndex', '');
                            if ($(menu.element).find('.t-group:visible').length == 0)
                                menu.nextItemZIndex = 100;
                        });
                        $ul.find('.t-group').stop(false, true);
                    }
                }, 100));
            });
        },

        mouseenter: function (e, element) {
            var $li = $(element);
            if (!this.openOnClick || this.clicked) {
                if (!contains(element, e.relatedTarget)) {
                    this.triggerEvent('open', $li);
                    this.open($li);

                    var parentItem = $li.parent().closest('.t-item')[0];

                    if (parentItem && !contains(parentItem, e.relatedTarget))
                        this.mouseenter(e, parentItem);
                }
            }

            if (this.openOnClick && this.clicked) {
                this.triggerEvent('close', $li);

                $li.siblings().each($.proxy(function (_, sibling) {
                    this.close($(sibling));
                }, this));
            }
        },

        mouseleave: function (e, element) {
            if (!this.openOnClick && !contains(element, e.relatedTarget)) {
                var $li = $(element);
                this.triggerEvent('close', $li);

                this.close($li);

                var parentItem = $li.parent().closest('.t-item')[0];

                if (parentItem && !contains(parentItem, e.relatedTarget))
                    this.mouseleave(e, parentItem);
            }
        },

        click: function (e, element) {
            //e.stopPropagation(); by moravej

            var $li = $(element);

            if ($li.hasClass('t-state-disabled')) {
                e.stopPropagation(); //by moravej
                e.preventDefault();
                return;
            }

            if ($t.trigger(this.element, 'select', { item: $li[0] })) {
                e.stopPropagation(); //by moravej
                e.preventDefault();
                return;
            }

            if (!$li.parent().hasClass('t-menu') || !this.openOnClick) {
                return;
            }

            e.stopPropagation(); //by moravej
            e.preventDefault();

            this.clicked = true;

            this.triggerEvent('open', $li);

            this.open($li);
        },

        documentClick: function (e, element) {
            if ($.contains(this.element, e.target))
                return;

            if (this.clicked) {
                this.clicked = false;
                $(this.element).children('.t-item').each($.proxy(function (i, item) {
                    this.close($(item));
                }, this));
            }
        },

        hasChildren: function ($li) {
            return $li.find('.t-group:first').length;
        },

        triggerEvent: function (eventName, $li) {
            if (this.hasChildren($li))
                $t.trigger(this.element, eventName, { item: $li[0] });
        }
    }

    $.fn.tMenu = function (options) {
        return $t.create(this, {
            name: 'tMenu',
            init: function (element, options) {
                return new $t.menu(element, options);
            },
            options: options
        });
    };

    // default options
    $.fn.tMenu.defaults = {
        orientation: 'horizontal',
        effects: $t.fx.slide.defaults(),
        openOnClick: false
    };
})(jQuery);