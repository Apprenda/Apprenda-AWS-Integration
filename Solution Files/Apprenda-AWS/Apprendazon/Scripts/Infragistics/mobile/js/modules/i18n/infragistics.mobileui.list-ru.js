/*!@license
* Infragistics.Web.MobileUI ListView localization resources 15.1.20151.2300
*
* Copyright (c) 2011-2015 Infragistics Inc.
*
* http://www.infragistics.com/
*
*/

$.ig = $.ig || {};

if (!$.ig.mobileListView) {
	$.ig.mobileListView = {};
	$.ig.mobileListViewFiltering = {};
	$.ig.mobileListViewLoadOnDemand = {};
	$.ig.mobileListViewSorting = {};

	$.ig.mobileListView.locale = {
		noSuchWidget: "Компонент не найден: ",
		optionChangeNotSupported: "Изменение этой опции не поддерживается после инициализации igListView:",
		emptyListText: "Список пуст!",
		goBackLabel: "Назад",
		detailsLabel: "Подробнее",
		searchTrayExpandLabel: "Сортировка/Фильтр",
		searchTrayCollapseLabel: "Свернуть"
	};
	$.ig.mobileListViewFiltering.locale = {
		keywordSearchLabel: "",
		keywordAllStateText: "Везде",
		filterPresetsLabel: "Фильтры:",
		searchBarPlaceHolder: "Фильтровать по...",
		filterAllStateText: "Все",
		showLabel: "Показать ",
		cancelButtonLabel: "Отмена",
		clearSearchButtonText: "Очистить"
	};
	$.ig.mobileListViewLoadOnDemand.locale = {
		loadMoreItemsLabel: "Загрузить еще",
		noMoreItemsLabel: "Загружено полностью"
	};
	$.ig.mobileListViewSorting.locale = {
		sortPresetsLabel: "Сортировки:",
		sortDefaultStateText: "По умолчанию",
		sortByLabel: "Сортировать по ",
		expandedCueText: "Нажмите, чтобы свернуть {0}",
		collapsedCueText: "Нажмите, чтобы развернуть {0}"
	};

}