﻿/*!@license
* Infragistics.Web.ClientUI Dialog localization resources 15.1.20151.2300
*
* Copyright (c) 2011-2015 Infragistics Inc.
*
* http://www.infragistics.com/
*
*/
(function($){$.ig=$.ig||{};if(!$.ig.Dialog){$.ig.Dialog={locale:{closeButtonTitle:"Close",minimizeButtonTitle:"Minimize",maximizeButtonTitle:"Maximize",pinButtonTitle:"Pin",unpinButtonTitle:"Unpin",restoreButtonTitle:"Restore"}}}})(jQuery);/*!@license
* Infragistics.Web.ClientUI Dialog 15.1.20151.2300
*
* Copyright (c) 2011-2015 Infragistics Inc.
*
* http://www.infragistics.com/
*
* Depends on:
* jquery-1.6.1.js
* jqueryui/1.8.11/jquery-ui.js
* jquery.ui.core.js
* jquery.ui.widget.js
* jquery.ui.mouse.js
* jquery.ui.draggable.js
* jquery.ui.resizable.js
* Example to use:
*	<script type="text/javascript">
*	$(function () {
*		$("#dialog1").igDialog();
*	});
*	</script>
*	<div id="dialog1"></div>
*/
(function($){var _lastTop,_iframe,_visCount=0,_modals=[],_lastZ=0,_maxZ=0,CLOSE=0,OPEN=1,MIN=2,MAX=3,PIN=4,UNPIN=5,RESTORE=6,_pos={my:"center",at:"center",collision:"fit",of:window,using:function(pos){if(pos.top<0){pos.top=0}if(pos.left<0){pos.left=0}var p=$(this).css(pos).offset();if(p.top<0){$(this).css("top",pos.top-p.top)}if(p.left<0){$(this).css("left",pos.left-p.left)}}},_isSrc=function(elem,src){return elem&&src&&(elem.has(src).length>0||elem[0]===src)},_notab=function(elem){return elem.attr("zIndex",-1).css("outline",0).attr("unselectable","on")},_toPx=function(elem,css){var val=elem.css(css);if(!val){return 0}css=parseFloat(val);if(val.indexOf("px")>0){css+=.7}else if(val.indexOf("em")>0){css*=12}else{return 0}return Math.floor(css)},_getPadding=function(elem,vert,margin){return _toPx(elem,(margin||"padding")+(vert?"Top":"Left"))+_toPx(elem,(margin||"padding")+(vert?"Bottom":"Right"))+_toPx(elem,"border"+(vert?"Top":"Left")+"Width")+_toPx(elem,"border"+(vert?"Bottom":"Right")+"Width")},_stopEvt=function(e){try{e.preventDefault();e.stopPropagation()}catch(ex){}};$.widget("ui.igDialog",{options:{mainElement:null,state:"opened",pinned:false,closeOnEscape:true,showCloseButton:true,showMaximizeButton:false,showMinimizeButton:false,showPinButton:false,pinOnMinimized:false,imageClass:null,headerText:null,showHeader:true,showFooter:false,footerText:null,dialogClass:null,container:null,height:null,width:300,minHeight:100,minWidth:150,maxHeight:null,maxWidth:null,draggable:true,position:null,resizable:true,tabIndex:0,openAnimation:null,closeAnimation:null,zIndex:null,modal:false,trackFocus:true,closeButtonTitle:null,minimizeButtonTitle:null,maximizeButtonTitle:null,pinButtonTitle:null,unpinButtonTitle:null,restoreButtonTitle:null,temporaryUrl:null,enableHeaderFocus:true,enableDblclick:"auto"},events:{stateChanging:null,stateChanged:null,animationEnded:null,focus:null,blur:null},css:{dialog:"ui-igdialog ui-dialog ui-widget ui-widget-content ui-corner-all",header:"ui-igdialog-header ui-dialog-titlebar ui-widget-header ui-corner-top ui-helper-clearfix",headerFocus:"ui-igdialog-header-focus ui-state-focus",headerMinimized:"ui-corner-bottom",headerText:"ui-igdialog-headertext ui-dialog-title",headerImage:"ui-igdialog-headerimage",headerTextMinimized:"ui-igdialog-headertext-minimized",headerButton:"ui-igdialog-headerbutton ui-corner-all ui-state-default",headerButtonHover:"ui-igdialog-headerbutton-hover ui-state-hover",close:"ui-igdialog-buttonclose",minimize:"ui-igdialog-buttonminimize",maximize:"ui-igdialog-buttonmaximize",pin:"ui-igdialog-buttonpin",closeIcon:"ui-igdialog-close-icon ui-icon ui-icon-close",minimizeIcon:"ui-igdialog-minimize-icon ui-icon ui-icon-minus",maximizeIcon:"ui-igdialog-maximize-icon ui-icon ui-icon-extlink",restoreIcon:"ui-igdialog-restore-icon ui-icon ui-icon-copy",pinIcon:"ui-igdialog-pin-icon ui-icon ui-icon-pin-s",unpinIcon:"ui-igdialog-unpin-icon ui-icon ui-icon-pin-w",footer:"ui-igdialog-footer ui-widget-header ui-corner-bottom ui-helper-clearfix",resizing:"ui-igdialog-resizing",dragging:"ui-igdialog-dragging",unmovable:"ui-igdialog-unmovable",overlay:"ui-igdialog-overlay ui-widget-overlay",contentIframe:"ui-igdialog-content-iframe",content:"ui-igdialog-content ui-widget-content ui-dialog-content"},_create:function(){var elem,self=this,elem0=self.element,el=elem0[0],url=el&&el.nodeName==="IFRAME"?el.src:null,o=self.options,state=o.state,parent,css=self.css;o.container=o.container||this.element.parent();parent=o.container;self._fixIE(elem0);self._old={position:elem0.css("position"),left:elem0.css("left"),top:elem0.css("top"),display:elem0.css("display"),visibility:elem0.css("visibility"),width:el.style.width,height:el.style.height};if(url){el.src=o.temporaryUrl||""}self._min=state==="minimized"||state===MIN;self._max=state==="maximized"||state===MAX;self._opened=state&&state!=="closed";self._oldDad=el.parentNode;self._next=self._oldDad?el.nextSibling:null;self._dad=parent;elem0=$("<div />");this.element.contents().appendTo(elem0);el=elem=this.element;elem.css({zIndex:o.zIndex||1e3,outline:0}).attr("tabIndex",o.tabIndex).keydown(function(e){if(o.closeOnEscape&&e.keyCode===$.ui.keyCode.ESCAPE){self.close(e);e.preventDefault()}if(e.keyCode!==$.ui.keyCode.TAB){return}self._tabTime=(new Date).getTime();if(!self._modal&&!self._max){return}var min,max,ti,next,iNext=-1,big=999999,iMin=big,iMax=-1,targ=e.target,ti0=self._getTabIndex(targ),shift=e.shiftKey,tabs=$(":tabbable",elem[0]),len=tabs.length,i=len;while(i-->0){ti=self._getTabIndex(el=tabs[i]);if(ti>iMax){iMax=ti;max=el}if(ti<=iMin){iMin=ti;min=el}if(ti===ti0){if(!next){next=el===targ;if(!next){iNext=i}}else if(iNext<0){iNext=i}}}if(iNext<0){i=len}iMin=shift?-1:big;while(i-->0){ti=self._getTabIndex(tabs[i]);if(ti>ti0&&ti<iMin&&!shift||ti<ti0&&ti>iMin&&shift){iMin=ti;iNext=i}}max=max||elem[0];min=min||max;self._nextTabElem=iNext>=0?tabs[iNext]:shift?max:min;if(targ===elem[0]||targ===min&&shift||targ===max&&!shift){_stopEvt(e);el=shift?max:min;try{el.focus()}catch(ex){}}}).mousedown(function(e){self.moveToTop(e)});el.addClass(css.dialog);if(o.dialogClass){el.addClass(o.dialogClass)}elem0.show().addClass(css.content).appendTo(el);if(url!==null){elem0[0].src=url;elem0.addClass(css.contentIframe)}self._modal=self._hasFocus=false;self._lastFoc="blur";self._doHeader();self._doFooter();self._doDraggable();self._doResizable();if(self._min){self._onMin(true,true,true)}if(self._max){o.pinned=false;self._onMax(true,true,true)}if(o.pinned){self._onPin(true,true,true)}if(self._opened){self._open()}else{elem.hide()}self._created=true;self._save()},_fixIE:function(elem){elem=elem.find("*");var n,e,i=elem.length;while(i-->0){e=elem[i];n=e.nodeName;if(n==="/INPUT"||n==="/IMG"){e.parentNode.removeChild(e)}}},destroy:function(){var self=this,elem0=this.element.children(".ui-igdialog-content");this._doClose(null,true);if(self._winResize){$(window).unbind("resize",self._winResize)}this.element.children(".ui-igdialog-header").remove();this.element.children(".ui-igdialog-footer").remove();elem0.contents().unwrap();this.element.removeClass(self.css.dialog).css(self._old);if(this.options.draggable){this.element.draggable("destroy")}if(this.options.resizable){this.element.resizable("destroy")}this.element.unbind();$.Widget.prototype.destroy.apply(this,arguments);return this},state:function(state){if(!arguments.length){return this.options.state}if((state==="minimized"||state===MIN)&&(!this._min||!this._opened)){if(!this._min){this._minimize()}else{this._open(null,1)}}if((state==="maximized"||state===MAX)&&(!this._max||!this._opened)){if(!this._max){this._maximize()}else{this._open(null,1)}}if((state==="opened"||state===OPEN)&&(this._min||this._max||!this._opened)){this._onMin();this._onMax();this._open();this.options.state=state}if((state==="closed"||!state)&&(this._min||this._max||this._opened)){this._onMin();this._onMax();this.close()}return this},mainElement:function(){return this.element},close:function(e){if(this._opened){this._doClose(e)}return this},open:function(){return this._open(null,1)},minimize:function(){if(!this._min){this._minimize()}return this},maximize:function(){if(!this._max){this._maximize()}return this},restore:function(){if(this._max){this._onMax()}if(this._min){this._onMin()}return this},pin:function(){if(!this.options.pinned){this._pin()}return this},unpin:function(){if(this.options.pinned){this._pin()}return this},getTopModal:function(){return _modals[_modals.length-1]},isTopModal:function(){return this.getTopModal()===this},moveToTop:function(e){var src,name,self=this,o=self.options,zi=o.zIndex,elem=self.element,zi0=self._created?null:zi,modal=o.modal,elem0=this.element[0],scrollTop=elem0.scrollTop,scrollLeft=elem0.scrollLeft;if($.ig&&$.ig.util&&$.ig.util.evtButton(e)){return}zi=zi||1e3;src=e?e.target:null;if(_isSrc(self._header,src)||_isSrc(self._footer,src)){name=src.nodeName;if(name!=="INPUT"&&name!=="BUTTON"){_stopEvt(e);self._setFocus()}}else if(e&&!this._hasFocus){self._setFocus()}_maxZ=Math.max(zi0||zi,_maxZ);if(o.pinned){return self}if(modal&&self._lastZ){elem=self._modalDiv;if(elem&&elem[0].offsetWidth<10){self._onResize()}return self}if(_lastTop===self&&(zi0||zi)>=_maxZ){return self}if(_lastTop&&!zi0){_lastTop.element.css("zIndex",_lastTop._lastZ||-1);_lastTop._save()}if(_lastZ>=_maxZ){_maxZ++}if(modal&&!zi0){_maxZ++;_maxZ++}_lastTop=self;self._lastZ=_lastZ=zi0||(modal||_modals.length>0?_maxZ:zi);if(!zi0){elem.css("zIndex",zi0||_maxZ);self._save()}elem0.scrollTop=scrollTop;elem0.scrollLeft=scrollLeft;if(modal){self._doModal(_maxZ)}return self},content:function(newContent){if(arguments.length===0){return this.element.children(".ui-igdialog-content")}this.element.children(".ui-igdialog-content").html(newContent)},_save:function(){var str,input,pos,o=this.options,name=o.inputName;if(!name){return}input=$('input[name="'+name+'"]');if(input.length===0){input=input.parents("form")[0]||document.forms[0];if(!input){return}input=$('<input type="hidden" name="'+name+'" />').appendTo(input)}str="s"+(o.pinned?"1":"")+(this._opened?this._min?2:this._max?3:1:0)+(o.width?":w"+o.width:"")+(o.height?":h"+o.height:"")+(":z"+this.element.css("zIndex")||o.zIndex);pos=o.position;if(pos&&pos.length===2){str+=":p"+pos[0]+","+pos[1]}input.val(str)},_open:function(e,raiseEvt){var self=this,o=self.options,elem=self.element,anim=self._min?null:o.openAnimation,arg={action:"open",owner:this};if(self._opened&&self._vis||raiseEvt&&!self._fireState(e,true,arg)){return self}if(!o.pinned){elem.css("position","absolute")}if(o.width!==null){elem.show()}self._opened=true;self._doSize(1);if(anim){elem.hide().show(anim,function(){self._trigger("animationEnded",e,arg)})}self._vis=true;_visCount++;self._trackFocus(elem);self.moveToTop(true);self._fixState();if(raiseEvt){self._fireState(e,false,arg)}self._save();return self},_initContainer:function(container,change){if(container){if(typeof container==="string"){container=$(container)}if(container&&container[0]){container=container[0]}}if(!container||!container.parentNode){container=this.element.parents("form")[0]||document.body}else if(container.nodeName!=="BODY"){var style=container.style,pos=style?style.position:null;if(style&&(!pos||pos==="static")){style.position="relative"}}if(change){this.element.appendTo(container)}return container},_fixState:function(){this.options.state=this._opened?this._min?"minimized":this._max?"maximized":"opened":"closed"},_minimize:function(e){return this._doState(e,{action:this._min?"restore":"minimize"},e?"minimize":null,"_onMin",true)},_maximize:function(e){return this._doState(e,{action:this._max?"restore":"maximize"},e?"maximize":null,"_onMax",true)},_pin:function(e){return this._doState(e,{action:this.options.pinned?"unpin":"pin"},e?"pin":null,"_onPin")},_close:function(e){return this._opened?this.close(e):this._open(e)},_getTabIndex:function(e){return isNaN(e=parseInt(e.tabIndex,10))||e<1?0:e},_doHeader:function(){var button,id,evts,i=4,self=this,header=self._header,o=self.options,txt=o.headerText,css=self.css;if(header){header.remove()}delete self._minHW;header=self._header=_notab($("<div />").addClass(css.header).css("display","block").prependTo(self.element)).dblclick(function(e){var dbl=o.enableDblclick;if(!dbl){return}if(self._min){self._doState(e,{action:"restore"},null,"_onMin",true)}else if(dbl===true||dbl==="auto"&&o.showMaximizeButton){self._doState(e,{action:self._max?"restore":"maximize"},null,"_onMax",true)}});if(o.imageClass){self._img=$("<span />").addClass(css.headerImage).addClass(o.imageClass).html("&nbsp;").appendTo(header)}self._headerText=$("<span />").addClass(css.headerText).html(txt||"&nbsp;").appendTo(header);evts={mouseover:function(){$(this).addClass(css.headerButtonHover)},mouseleave:function(){$(this).removeClass(css.headerButtonHover)},mousedown:function(e){this._mdb=$.ig&&$.ig.util&&$.ig.util.evtButton(e)},click:function(e){if(!e||this._mdb){return}try{self["_"+$(this).attr("data-id")](e)}catch(ex){}_stopEvt(e)},touchstart:function(e){this._drag=null;_stopEvt(e)},touchmove:function(e){this._drag=1;_stopEvt(e)},touchend:function(){if(!this._drag){$(this).trigger("click")}}};while(i-->=0){if(i===3&&o.showCloseButton){id="close"}else if(i===2&&o.showMaximizeButton){id="maximize"}else if(i===1&&o.showMinimizeButton){id="minimize"}else{id=i===0&&o.showPinButton?"pin":null}if(id){button=$("<a />").addClass(css.headerButton+" "+css[id]).attr("data-id",id).attr("href","#").attr("role","button").bind(evts).appendTo(header);$("<span />").addClass(css[id+"Icon"]).appendTo(button);self._loc(button,i===3?CLOSE:i===2?MAX:i===1?MIN:PIN)}}if(!o.showHeader){header.hide()}},_doFooter:function(){var self=this,o=self.options,txt=o.footerText,css=self.css;if(self._footer){self._footer.remove();delete self._footer}if(o.showFooter){self._footer=_notab($("<div />").addClass(css.footer).css("display","block").html(txt||"&nbsp").appendTo(self.element))}},_onMin:function(e,noSize,noFocus){var but,o=this.options,bar=this._footer,css=this.css,header=this._header,min=e&&e.type?!this._min:!!e;if(min===this._min&&this._created){return}this._min=min;if(min&&o.pinOnMinimized){this._onPin(min,true,true)}but=header.find("."+css.minimize);but.find("*").removeClass(min?css.minimizeIcon:css.restoreIcon).addClass(min?css.restoreIcon:css.minimizeIcon);if(e&&e.type&&min&&this._max){this._onMax(false,true,true)}this._loc(but,min?RESTORE:MIN);if(min){header.addClass(css.headerMinimized);if(bar){bar.hide()}}else{header.removeClass(css.headerMinimized);if(bar){bar.show()}}if(!noSize&&this._vis){this._doSize()}if(!noFocus&&this._vis){this._setFocus()}this._save()},_onMax:function(e,noSize,noFocus){var but,o=this.options,header=this._header,css=this.css,max=e&&e.type?!this._max:!!e;if(max===this._max&&this._created){return}this._max=max;if(!max){this._restoreHtml()}else{this._originalParent=this.element.parent();this.element.appendTo(document.body)}but=header.find("."+css.maximize);but.find("*").removeClass(max?css.maximizeIcon:css.restoreIcon).addClass(max?css.restoreIcon:css.maximizeIcon);this._loc(but,max?RESTORE:MAX);if(max){if(this._min){this._onMin(false,true,true)}if(o.pinned){this._onPin(false,true,true)}}if(max){header.addClass(css.unmovable)}else{header.removeClass(css.unmovable)}if(!noSize&&this._vis){this._doSize()}if(!noFocus&&this._vis){this._setFocus()}this._save()},_onPin:function(e,noSize,noFocus){var but,elem,parent,dad,pos,old=this._old,next=this._next,css=this.css,header=this._header,o=this.options,pin=e&&e.type?!o.pinned:!!e;if(pin===o.pinned&&this._created){return}o.pinned=pin;but=header.find("."+css.pin);but.find("*").removeClass(pin?css.pinIcon:css.unpinIcon).addClass(pin?css.unpinIcon:css.pinIcon);if(this._max&&pin){this._onMax(false,false,true)}this._loc(but,pin?UNPIN:PIN);if(pin){header.addClass(css.unmovable)}else{header.removeClass(css.unmovable)}elem=this.element;if(pin){pos=old.position;if(this._resize&&(pos==="static"||!pos)){pos="relative"}this._pinPos=pos={position:pos,left:old.left,top:old.top}}else{pos={position:"absolute"}}elem.css(pos);parent=elem.parent()[0];dad=pin?this._oldDad:this._dad;if(dad&&dad!==parent){if(pin&&next&&next.parentNode===dad){elem.insertBefore(next)}else{elem.appendTo(dad)}}if(!noFocus&&this._vis){this._setFocus()}if(!noSize&&this._vis){if(!pin){this._doSize(1)}else{this._doModal()}}this._save()},_doClose:function(e,destroy){var i,self=this,elem=self.element,arg={action:"close"},o=self.options,anim=self._min||destroy?null:o.closeAnimation;if(!self._opened||!destroy&&!self._fireState(e,true,arg,e?"close":null)){return}self._trackFocus(elem,1);self._restoreHtml();if(_lastTop===self){_lastTop=null}self._fireFoc(false);self._hasFocus=false;delete self._lastZ;self._vis=self._opened=false;if(destroy){o.modal=false}self._doModal();if(anim){elem.hide(anim,function(){self._trigger("animationEnded",e,arg)})}else if(!destroy){elem.hide()}if(!destroy){self._fixState();self._fireState(e,false,arg)}if(--_visCount<1){_visCount=_lastZ=_maxZ=0}else if(_visCount===(i=_modals.length)){_modals[i-1]._setFocus()}self._save()},_fireState:function(e,before,arg,but){if(before){var o=this.options;arg.oldState=o.state;arg.oldPinned=o.pinned;arg.owner=this;if(but){arg.button=but}}return this._created?this._trigger("stateChang"+(before?"ing":"ed"),e,arg):true},_doState:function(e,arg,but,fnName,show){if(this._fireState(e,true,arg,but)){this[fnName](e||{type:1});if(show&&!this._opened){this._open(null,true)}this._fixState();if(this._created){this._trigger("stateChanged",e,arg)}}return this},_fireFoc:function(foc,e){var name=foc?"focus":"blur";if(name!==this._lastFoc){this._trigger(this._lastFoc=name,e,{owner:this});if(this.options.enableHeaderFocus){name=this.css.headerFocus;if(foc){this._header.addClass(name)}else{this._header.removeClass(name)}}}},_trackFocus:function(elem,remove){var self=this,focusEvt=self._focusEvt,track=self.options.trackFocus;if(!focusEvt&&!track){return}if(remove){if(self._focBind){self._focBind.unbind(focusEvt);delete self._focBind}return}if(!focusEvt){focusEvt=function(e){var elems,old=self._focBind,foc=e.type==="focus";if(self._isDatePickerOpened()){return}if(!foc&&old&&elem){elems=elem.find("*").not(old);if(elems.length){self._focBind=old.add(elems);elems.bind(focusEvt)}}self._hasFocus=foc;setTimeout(function(){var focusTo=self.getTopModal(),elem=self.element;if(elem&&focusTo&&!self._hasFocus&&!foc&&_lastTop===self){if(self._max||focusTo===self){focusTo=self._nextTabElem||elem[0]}else{focusTo=self._tabTime&&(new Date).getTime()-self._tabTime<200?elem[0]:null}if(focusTo){self._setFocus(focusTo)}}self._fireFoc(self._hasFocus,e)},50)};focusEvt=self._focusEvt={focus:focusEvt,blur:focusEvt}}if(track&&elem){self._focBind=elem.find("*").add(elem).bind(focusEvt)}},_isDatePickerOpened:function(){return $("#ui-datepicker-div")[0]&&$("#ui-datepicker-div").css("display")==="block"},_setFocus:function(elem){var self=this;setTimeout(function(){try{if(!self._hasFocus){if(!self.options.trackFocus){self._hasFocus=true}elem=elem||self.element[0];elem.focus()}}catch(ex){}},100)},_restoreHtml:function(){var html,old=this._oldHtml,parent=this._originalParent;if(parent){this.element.appendTo(parent);this._originalParent=null}if(old){html=old.html;if(html.style){html.style.overflow=old.overflow}html.scrollLeft=old.scrollLeft;html.scrollTop=old.scrollTop;delete this._oldHtml}},_touch:function(elem,name){var start,self=this,evt=function(evt,type){var act,e=evt.originalEvent,touches=e?e.touches:null,one=touches&&touches.length===1;if(one&&type){_stopEvt(evt)}one=one&&type==="move";if(start){start=one?start:null;act=one?"Drag":"Stop"}else if(one){start=true;elem.trigger("mouseover");act="Start"}if(act){e=self.element.data(name);act="_mouse"+act;if(e&&e[act]){evt.pageX=one?touches[0].pageX:0;evt.pageY=one?touches[0].pageY:0;e[act](evt)}}};elem.bind({touchstart:function(e){evt(e,"start")},touchmove:function(e){evt(e,"move")},touchend:function(e){evt(e)}})},_doDraggable:function(){var self=this,o=self.options,elem=self.element;if(elem.draggable&&o.draggable){self._touch(self._header,"draggable");elem.draggable({cancel:".ui-igdialog-content, .ui-igdialog-headerbutton",handle:".ui-igdialog-header",containment:"document",start:function(){if(o.pinned||self._max){return false}$(this).addClass(self.css.dragging)},stop:function(e,ui){var doc=$(document);o.position=[ui.position.left-doc.scrollLeft(),ui.position.top-doc.scrollTop()];$(this).removeClass(self.css.dragging);self._save()}})}},_doResizable:function(){var elems,r,i=0,self=this,o=self.options,elem=self.element;if(!elem.resizable){return}self._resize=o.resizable;if(!self._resize){return}elem.css("position",elem.css("position")).resizable({cancel:"."+self.css.content,containment:"document",alsoResize:self.element.children(".ui-igdialog-content"),maxWidth:o.maxWidth,maxHeight:o.maxHeight,minWidth:self._minWidth(),minHeight:o.minHeight,handles:typeof o.resizable==="string"?o.resizable:"n,e,s,w,se,sw,ne,nw",start:function(){$(this).addClass(self.css.resizing);if(o.pinned&&self._pinPos){elem.css(self._pinPos)}},resize:function(){self._fixCaption();if(o.pinned&&self._pinPos){elem.css(self._pinPos)}},stop:function(){$(this).removeClass(self.css.resizing);o.height=$(this).height();o.width=$(this).width();self._save()}}).find(".ui-resizable-se").addClass("ui-icon ui-icon-grip-diagonal-se");r=elem.data("resizable")||elem.data("ui-resizable");if(r){if(!r._dragFix){r._dragFix=r._mouseDrag;r._mouseDrag=function(e){var x,y,d=r.parentData;if(d&&e){x=e.pageX;y=e.pageY;if(x<=d.left||y<=d.top||x>=d.left+d.width||y>=d.top+d.height){return false}}return r._dragFix(e)}}elems=r._handles;i=elems.length}while(i-->0){self._touch($(elems[i]),"resizable")}},_toPx:function(val,height){if(typeof val==="number"){return val}if(!val){return height?val:0}val=val.toString();var elem,num=parseInt(val,10);if(isNaN(num)){return 0}if(val.indexOf("m")>0||val.indexOf("e")>0||val.indexOf("i")>0||val.indexOf("t")>0){elem=$("<div />").css({visibility:"hidden",width:val}).appendTo(this._dad);num=elem.width();elem.remove()}else if(val.indexOf("%")>0){val=this._winRect(1);val=height?val.height:val.width;return Math.floor(num*val/100)}return num},_doSize:function(fixPos){var self=this,o=self.options,max=self._max,pos=max?[0,0]:o.position,resize=self._resize?".ui-resizable-handle":null,elem0=self.element.children(".ui-igdialog-content"),elem=self.element;if(resize){if(self._min||max){$(resize,elem).hide()}else{$(resize,elem).show()}}self._headerText.css("width",0);if(self._min){elem0.hide();self._fixCaption(elem)}else if(max){elem0.show().css({width:"auto",height:"auto"});elem.css({width:100,height:50})}else if(o.width!==null){this._doSizePX(elem0,elem,Math.max(self._minWidth(),self._toPx(o.width)),self._toPx(o.height,true),o.minHeight);if(resize){elem.resizable("option","minHeight",o.minHeight)}}if(o.width===null){this._fixCaption(elem);elem.show()}if(!o.pinned&&(fixPos||max||self._oldMax)){self._oldMax=max;if(max){self._onResize()}if(elem.position){if(pos){if(pos.left!==undefined&&pos.top!==undefined){pos=[pos.left,pos.top]}if(pos&&pos.length>1){if(typeof pos[0]!=="number"){pos[0]=parseInt(pos[0])}if(typeof pos[1]!=="number"){pos[1]=parseInt(pos[1])}if(isNaN(pos[0])||isNaN(pos[1])){pos={}}else{if($.ig.util.jQueryUIMainVersion<=1&&$.ig.util.jQueryUISubVersion<9){pos={my:"left top",at:"left top",offset:pos[0]+" "+pos[1]}}else{pos={my:"left+"+pos[0]+" top+"+pos[1],at:"left top"}}}}pos=$.extend({},_pos,pos)}elem.css({top:0,left:0}).position(pos||_pos)}}self._doModal();self._save()},_doSizePX:function(elem0,elem,width,height,minHeight){elem0.show().css({width:"auto",height:0,minHeight:0});var zeroHeight=elem.css({width:width,height:"auto",display:"block"}).height();this._fixCaption(elem);if(typeof height==="string"){if(height.indexOf("px")>0){height=parseInt(height,10)}}if(typeof height!=="number"){height=elem0.css("height","auto").height()+zeroHeight}height=Math.max(minHeight,height);elem0.height(Math.max(height-zeroHeight,0));minHeight=height-elem[0].offsetHeight;if(minHeight>0){elem0.height(Math.max(height-zeroHeight+minHeight,0))}},_onResize:function(){var rect,self=this,div=self.isTopModal()?self._modalDiv:null;if(!self._winResize){$(window).bind("resize",self._winResize=function(){setTimeout(function(){self._onResize()},50)})}if(!self._opened||self.options.pinned){return}if(div){div.hide();self._doIframe(div,1)}if(self._max){self._doMaxSize(self.element)}if(div){rect=self._winRect();div.css({width:rect.maxWidth-1,height:rect.maxHeight-1}).show();self._doIframe(div)}},_minHeaderWidth:function(){var outerWidth,elem,width=this._minHW,elems=this._header.children().not(this._headerText),i=elems.length;if(!width){width=3+_getPadding(this._header);while(--i>=0){elem=elems[i];try{outerWidth=$(elem).outerWidth(true)}catch(ex){}width+=1+(outerWidth&&outerWidth>2&&outerWidth<100?outerWidth:elem.offsetWidth)}this._minHW=width}return width},_minWidth:function(){if(!this._minW){this._minW=this._minHeaderWidth()}return Math.max(this.options.minWidth,this._minW)},_fixCaption:function(elem){var width,widths,top,len,topi,j=0,i=-1,header=this._header,cap=this._headerText,minCss=this.css.headerTextMinimized;if(this._min){cap.css("width","").addClass(minCss);if(!elem){return}elem.css({height:"auto",width:"auto",display:"inline-block"});widths=_getPadding(header)+3;cap=header.children();len=cap.length;while(++i<len){widths+=cap[i].offsetWidth+_toPx($(cap[i]),"marginLeft")+_toPx($(cap[i]),"marginRight")}while(j++<2){elem.css("width",widths);widths+=2;i=len;while(i-->0){topi=cap[i].offsetTop;if(i>0&&i<len-1&&Math.abs(top-topi)>4){break}top=topi}if(i<0){j=4}}return}cap.removeClass(minCss);try{width=header.innerWidth()-3}catch(ex){}if(!width||width>1e3){width=header[0].clientWidth-4}width=Math.max(1,width-this._minHeaderWidth());cap.css("width","auto");if(cap[0].offsetWidth*1.3>width){cap.css("width",width)}},_doMaxSize:function(elem){var html,old=this._oldHtml,elem0=this.element.children(".ui-igdialog-content"),rect=this._winRect(),paddingX=_getPadding(elem),paddingY=_getPadding(elem,1);html=rect.html;if(!old){this._oldHtml=old={html:html,scrollLeft:html.scrollLeft,scrollTop:html.scrollTop};html.scrollLeft=html.scrollTop=0;if(html.style){old.overflow=html.style.overflow;html.style.overflow="hidden";if(rect.maxWidth>rect.width||rect.maxHeight>rect.height){rect=this._winRect(1)}}}this._doSizePX(elem0,elem,rect.width-paddingX-1,rect.height-paddingY-1,0)},_winRect:function(sizeOnly){var size,docElem,width,height,widthOk,heightOk,maxWidth=0,maxHeight=0,big=999999,win=window,doc=win.document,body=doc.body,html=body;while(html&&html.nodeName!=="HTML"){html=html.parentNode}if(!html){html=body}docElem=doc.documentElement||html;size=doc.compatMode!=="CSS1Compat"&&$.ig.util.isIE?body:html;width=size.clientWidth;height=size.clientHeight;if(sizeOnly){return{width:width,height:height}}if(width&&width>50){maxWidth=width;maxHeight=height}else{width=height=big}widthOk=html.scrollWidth;heightOk=html.scrollHeight;if(widthOk&&heightOk){maxWidth=Math.max(maxWidth,widthOk);maxHeight=Math.max(maxHeight,heightOk)}maxWidth=Math.max(maxWidth,body.scrollWidth);maxHeight=Math.max(maxHeight,body.scrollHeight);widthOk=body.offsetWidth;heightOk=body.offsetHeight;maxWidth=Math.max(maxWidth,widthOk);maxHeight=Math.max(maxHeight,heightOk);return{width:width===big?widthOk:width,height:height===big?heightOk:height,maxWidth:maxWidth,maxHeight:maxHeight,html:html}},_doIframe:function(div,hide){var src="javascript";if(!_iframe){_iframe=_notab($("<iframe />").attr("frameBorder",0).attr("scrolling","no").attr("src",src+":''").css({position:"absolute",filter:"alpha(opacity=50)",opacity:0}))}if(_iframe.parent()[0]!==div.parent()[0]){_iframe.css({width:"1px",height:"1px",marginLeft:div.css("marginLeft"),marginTop:div.css("marginTop"),left:div.css("left"),top:div.css("top"),zIndex:div.attr("zIndex")-1}).insertBefore(div)}_iframe.css({width:hide?"1px":div.css("width"),height:hide?"1px":div.css("height")})},_doModal:function(zi){var i,pos,on,obj,len=_modals.length,self=this,o=self.options,elem=self.element,div=self._modalDiv;on=o.modal&&!o.pinned&&!self._min&&self._opened;i=$.inArray(self,_modals);if(self._modal===on){if(zi&&div){div.css("zIndex",zi-1);self._onResize()}if(!on&&!_lastTop&&len>0){_modals[len-1].moveToTop()}return}if(i<0&&on){if(len>0){_modals[len-1]._modalDiv.hide()}_modals.push(self)}if(i>=0&&!on){if(i>0&&i+1===len){obj=_modals[i-1]}_modals.splice(i,1)}self._modal=on;if(on){self._modalDiv=div=_notab($("<div />").css({position:"absolute",left:0,top:0,zIndex:_maxZ-1}).addClass(self.css.overlay).mousedown(function(e){self._setFocus();_stopEvt(e)}).insertBefore(elem));pos=div.offset();div.css({marginLeft:-pos.left+"px",marginTop:-pos.top+"px"});self._onResize()}else{div.remove();_iframe.remove();delete self._modalDiv;if(obj){obj.moveToTop()}}},_loc:function(but,state){state=(state===MIN?"minimize":state===MAX?"maximize":state===RESTORE?"restore":state===CLOSE?"close":state===PIN?"pin":state===UNPIN?"unpin":"open")+"ButtonTitle";var val=this.options[state]||($.ig&&$.ig.Dialog&&$.ig.Dialog.locale?$.ig.Dialog.locale[state]:null)||"";but.attr("title",val).attr("longdesc",val)},_setOption:function(key,val){var pos,size,drag,resize,elem=this.element,o=this.options,container=key==="container";if(!elem||!key||o[key]===val||key==="mainElement"){return this}if(key==="state"){return this.state(val)}if(key==="pinned"){return this._pin()}if(container){if(o.draggable&&elem.draggable){elem.draggable("destroy");drag=true}if(o.resizable&&elem.resizable){elem.resizable("destroy");resize=true}}$.Widget.prototype._setOption.apply(this,arguments);if(typeof val==="function"){return this}if(container){this._initContainer(val,1);if(drag){this._doDraggable()}if(resize){this._doResizable()}}if(key==="draggable"){if(val){this._doDraggable()}else if(elem.draggable){elem.draggable("destroy")}}if(key==="resizable"){if(val){this._doResizable()}else if(this._resize){this._resize=val;elem.resizable("destroy")}}if(key==="modal"){this._doModal()}if(key.indexOf("Button")>0||key==="image"||key==="headerText"||key==="showHeader"){this._doHeader();size=true}if(key.indexOf("ooter")>0){this._doFooter();size=true}if(key==="tabIndex"){elem.attr("tabIndex",val)}if(key==="zIndex"){elem.css("zIndex",val);this._save()}if(this._vis){pos=key==="position";if(container||size||pos||key.indexOf("idth")>0||key.indexOf("eight")>0){this._doSize(pos||container)}}if(key.indexOf("Foc")>0){this._header.removeClass(this.css.headerFocus);if(key==="trackFocus"&&val!==(this._focBind?true:false)){if(this._opened){this._doClose();this._open()}else{this._open();this._doClose()}}}return this}});$.extend($.ui.igDialog,{version:"15.1.20151.2300"})})(jQuery);(function($){$(document).ready(function(){var wm=$("#__ig_wm__").length>0?$("#__ig_wm__"):$('<div id="__ig_wm__"></div>').appendTo(document.body);wm.css({position:"fixed",bottom:0,right:0,zIndex:1e3}).addClass("ui-igtrialwatermark")})})(jQuery);