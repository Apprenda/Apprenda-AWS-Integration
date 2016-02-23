/*!@license
* Infragistics.Web.ClientUI Grid localization resources 15.1.20151.2300
*
* Copyright (c) 2011-2015 Infragistics Inc.
*
* http://www.infragistics.com/
*
*/

/*global jQuery */
(function ($) {
$.ig = $.ig || {};

if (!$.ig.Grid) {
	$.ig.Grid = {};

	$.extend($.ig.Grid, {

		locale: {
		    noSuchWidget: "{featureName} не распознано. Проверьте, что такая функция существует и правильность написания.",
			autoGenerateColumnsNoRecords: "Установлена опция autoGenerateColumns, но исходные данные не содержат записей и поэтому невозможно определить поля",
			optionChangeNotSupported: "Нельзя редактировать {optionName} после инициализации. Это значение следует задать в процессе инициализации.",
			optionChangeNotScrollingGrid: "Нельзя редактировать {optionName} после инициализации, поскольку на начальном этапе ваша сетка не будет прокручиваться и потребуется полная перерисовка сетки. Этот параметр следует задать в процессе инициализации.",
			noPrimaryKeyDefined: "Первичный ключ не определен. Чтобы использовать такие возможности как редактирование (Grid Updating), необходимо установить первичный ключ.",
			indexOutOfRange: "Недопустимое значение указанного индекса строки. Следует указать индекс строки в диапазоне от {0} до {max}.",
			noSuchColumnDefined: "Указанный идентификатор колонки не соответствует ни одной колонке в таблице.",
			columnIndexOutOfRange: "Недопустимое значение указанного индекса столбца. Следует указать индекс столбца в диапазоне от {0} до {max}.",
			recordNotFound: "Не удалось найти запись с идентификатором {id} в представленных данных. Проверьте идентификатор, который вы использовали для поиска и, при необходимости, выполните правку.",
			columnNotFound: "Не удалось найти столбец с ключом {key}. Проверьте ключ, который вы использовали для поиска и, при необходимости, выполните правку.",
			colPrefix: "Колонка ",
			columnVirtualizationRequiresWidth: "Опция virtualization / columnVirtualization установлена в true, но ширина таблицы не может быть определена по колонкам. Необходимо установить a) ширину таблицы (width), b) defaultColumnWidth, c) ширину каждой колонки",
			virtualizationRequiresHeight: "Опция virtualization установлена в true, что требует определения высоты таблицы (height).",
			colVirtualizationDenied: "Установка опции columnVirtualization разрешена только при фиксированной виртуализации.",
			noColumnsButAutoGenerateTrue: "Опция autoGenerateColumns установлена в false, но таблица не содержит определений колонок. Установите autoGenerateColumns в true или определите колонки программно.",
			noPrimaryKey: "Для компонента igHierarchicalGrid необходимо задать первичный ключ.",
			templatingEnabledButNoTemplate: "Опция jQueryTemplating установлена в true, но опция rowTemplate не определена.",
			expandTooltip: "Развернуть",
			collapseTooltip: "Свернуть",
			featureChooserTooltip: "Селектор функций",
			movingNotAllowedOrIncompatible: "Невозможно переместить колонку. Колонка не найдена или конечный результат несовместим с заданным расположением колонок.",
			allColumnsHiddenOnInitialization: "Все столбцы сетки не могут быть скрыты. Задайте хотя бы один столбец, который будет отображаться.",
			virtualizationNotSupportedWithAutoSizeCols: "Для Virtualization требуются конфигурация ширины столбца, отличная от '*'. Следует задать численное значение ширины столбца в пикселях.",
			columnVirtualizationNotSupportedWithPercentageWidth: "Если ширина сетки задана в процентах, виртуализация столбцов не поддерживается.",
			mixedWidthsNotSupported: "Смешанные/частичные установки ширины столбца не поддерживаются. Не поддерживаются варианты, когда ширина некоторых столбцов задана в процентах, а ширина других – в пикселах (либо вообще не задана)."
		}
	});

	$.ig.GridFiltering = $.ig.GridFiltering || {};

	$.extend($.ig.GridFiltering, {
		locale: {
			startsWithNullText: "Начинается с...",
			endsWithNullText: "Оканчивается на...",
			containsNullText: "Содержит...",
			doesNotContainNullText: "Не содержит...",
			equalsNullText: "Равно...",
			doesNotEqualNullText: "Не равно...",
			greaterThanNullText: "Больше...",
			lessThanNullText: "Меньше...",
			greaterThanOrEqualToNullText: "Больше или равно...",
			lessThanOrEqualToNullText: "Меньше или равно...",
			onNullText: "На...",
			notOnNullText: "Не на...",
			afterNullText: "После",
			beforeNullText: "До",
			emptyNullText: "Пусто",
			notEmptyNullText: "Не пусто",
			nullNullText: "Null",
			notNullNullText: "Не null",
			startsWithLabel: "Начинается с",
			endsWithLabel: "Оканчивается на",
			containsLabel: "Содержит",
			doesNotContainLabel: "Не содержит",
			equalsLabel: "Равно",
			doesNotEqualLabel: "Не равно",
			greaterThanLabel: "Больше",
			lessThanLabel: "Меньше",
			greaterThanOrEqualToLabel: "Больше или равно",
			lessThanOrEqualToLabel: "Меньше или равно",
			trueLabel: "True",
			falseLabel: "False",
			afterLabel: "После",
			beforeLabel: "До",
			todayLabel: "Сегодня",
			yesterdayLabel: "Вчера",
			thisMonthLabel: "В этом месяце",
			lastMonthLabel: "В прошлом месяце",
			nextMonthLabel: "В следующем месяце",
			thisYearLabel: "В этом году",
			lastYearLabel: "В прошлом году",
			nextYearLabel: "В следующем году",
			clearLabel: "Очистить фильтр",
			noFilterLabel: "Нет",
			onLabel: "На",
			notOnLabel: "Не на",
			advancedButtonLabel: "Дополнительно",
			filterDialogCaptionLabel: "РАСШИРЕННЫЙ ФИЛЬТР",
			filterDialogConditionLabel1: "Показать записи соответствующие ",
			filterDialogConditionLabel2: " из следующих условий",
			filterDialogConditionDropDownLabel: "Условие фильтрации",
			filterDialogOkLabel: "Поиск",
			filterDialogCancelLabel: "Отмена",
			filterDialogAnyLabel: "ОДНОМУ",
			filterDialogAllLabel: "КАЖДОМУ",
			filterDialogAddLabel: "Добавить",
			filterDialogErrorLabel: "Превышено допустимое количество условий.",
			filterDialogCloseLabel: "Закрыть диалоговое окно фильтрации",
			filterSummaryTitleLabel: "Результаты поиска",
			filterSummaryTemplate: "${matches} найденных записей",
			filterDialogClearAllLabel: "Очистить ВСЕ",
			tooltipTemplate: "Отфильтровано по ${condition}",
			// M.H. 13 Oct. 2011 Fix for bug #91007
			featureChooserText: "Спрятать фильтр",
			featureChooserTextHide: "Показать фильтр",
			// M.H. 17 Oct. 2011 Fix for bug #91007
			featureChooserTextAdvancedFilter: "Усиленный фильтр",
			virtualizationSimpleFilteringNotAllowed: "Когда горизонтальная виртуализация включена, упрощенный режим фильтрации (фильтровочный ряд) не поддерживается. Установите 'усиленный' режим и/или не устанавливайте advancedModeEditorsVisible",
			featureChooserNotReferenced: "Скрипт модуля выбора опций отсутствует. Чтобы избежать этой ошибки, добавьте ссылку на файл ig.ui.grid.featurechooser.js, используйте загрузчик или ссылку на комбинированный скрипт."
		}
	});

	$.ig.GridGroupBy = $.ig.GridGroupBy || {};

	$.extend($.ig.GridGroupBy, {
		locale: {
			emptyGroupByAreaContent: "Поместите колонку сюда или {0} для группировки",
			emptyGroupByAreaContentSelectColumns: "выберите колонки",
			emptyGroupByAreaContentSelectColumnsCaption: "выберите колонки",
			expandTooltip: "Развернуть групповой ряд",
			collapseTooltip: "Свернуть групповой ряд",
			removeButtonTooltip: "Убрать группировочную колонку",
			featureChooserText: "Разгруппировать",
			featureChooserTextHide: "Группировать",
			modalDialogCaptionButtonDesc: "Нажмите для сортировки по возрастанию",
			modalDialogCaptionButtonAsc: "Нажмите для сортировки по убыванию",
			modalDialogCaptionButtonUngroup: "Нажмите для разгруппировки",
			modalDialogGroupByButtonText: "Добавить",
			modalDialogCaptionText: 'Добавить к группировке',
			modalDialogDropDownLabel: 'Показ:',
			modalDialogClearAllButtonLabel: 'очистить все',
			modalDialogRootLevelHierarchicalGrid: 'первый уровень',
			modalDialogDropDownButtonCaption: "Нажмите чтобы показать/спрятать",
			modalDialogButtonApplyText: 'Готово',
			modalDialogButtonCancelText: 'Отмена',
			fixedVirualizationNotSupported: 'Функция GroupBy не работает с фиксированной виртуализацией.',
			summaryRowTitle: 'Группирование строки итогов'
		}
	});

	$.ig.GridHiding = $.ig.GridHiding || {};

	$.extend($.ig.GridHiding, {
		locale: {
			columnChooserDisplayText: "Выбор колонок",
			hiddenColumnIndicatorTooltipText: "Скрытые колонки",
			columnHideText: "Скрыть",
			columnChooserCaptionLabel: "Выбор колонок",
			columnChooserCheckboxesHeader: "видеть",
			columnChooserColumnsHeader: "колонка",
			columnChooserCloseButtonTooltip: "Закрыть",
			hideColumnIconTooltip: "Скрыть",
			featureChooserNotReferenced: "Скрипт модуля выбора опций отсутствует. Чтобы избежать этой ошибки, добавьте ссылку на файл ig.ui.grid.featurechooser.js, используйте загрузчик или ссылку на комбинированный скрипт.",
			columnChooserShowText: "Показ",
			columnChooserHideText: "Скрыть",
			columnChooserResetButtonLabel: "сброс",
			columnChooserButtonApplyText: 'Готово',
			columnChooserButtonCancelText: 'Отмена'
		}
	});

		$.ig.GridResizing = $.ig.GridResizing || {};

		$.extend($.ig.GridResizing, {
			locale: {
			    noSuchVisibleColumn: "Не удалось найти видимый столбец с заданным ключом. Изменение размера возможно только для видимых столбцов.",
			    resizingAndFixedVirtualizationNotSupported: "Если включена виртуализация либо виртуализация столбцов и для параметра virtualizationMode установлено значение фиксированная, функция изменения размера не работает. Чтобы избежать этого, установите для параметра virtualizationMode значение 'continuous' либо используйте только опцию rowVirtualization."
			}
		});

	$.ig.GridPaging = $.ig.GridPaging || {};

	$.extend($.ig.GridPaging, {

		locale: {
			pageSizeDropDownLabel: "Показать ",
			pageSizeDropDownTrailingLabel: "записей",
			//pageSizeDropDownTemplate: "Show ${dropdown} records",
			nextPageLabelText: "след",
			prevPageLabelText: "пред",
			firstPageLabelText: "",
			lastPageLabelText: "",
			currentPageDropDownLeadingLabel: "Стр",
			currentPageDropDownTrailingLabel: "из ${count}",
			//currentPageDropDownTemplate: "Стр ${dropdown} из ${count}",
			currentPageDropDownTooltip: "Выберите номер страницы",
			pageSizeDropDownTooltip: "Выберите количество записей на страницу",
			pagerRecordsLabelTooltip: "Текущий спектр записей",
			prevPageTooltip: "перейти к предыдущей странице",
			nextPageTooltip: "перейти к следующей странице",
			firstPageTooltip: "перейти к первой странице",
			lastPageTooltip: "перейти к последней странице",
			pageTooltipFormat: "страница ${index}",
			    pagerRecordsLabelTemplate: "${startRecord} - ${endRecord} из ${recordCount} записей",
			    invalidPageIndex: "Недопустимый индекс страницы: индекс должен быть больше или равен 0 и не должен превышать количество страниц"
		}
	});

    $.ig.GridSelection = $.ig.GridSelection || {};

    $.extend($.ig.GridSelection, {
        locale: {
        	persistenceImpossible: "Сохранение состояния селекции при обновлении возможно только при установки опции primaryKey в igGrid. Чтобы избежать этой ошибки, пожалуйста, установите первичный ключ или отключите сохранение состояния."
        }
    });

	$.ig.GridRowSelectors = $.ig.GridRowSelectors || {};

	$.extend($.ig.GridRowSelectors, {

		locale: {
			selectionNotLoaded: "igGridSelection не включена. Чтобы избежать этого сообщения об ошибке, включите опцию селекции (Selection) в таблице или установите опцию requireSelection в модуле Row Selectors в false.",
			columnVirtualizationEnabled: "Если включена виртуализация столбцов, опция igGridRowSelectors не поддерживается. Для того чтобы предотвратить получение этого сообщения об ошибке, включите только виртуализацию строк, активировав свойство сетки 'rowVirtualization', либо измените режим виртуализации на 'continuous'."
		}
	});

	$.ig.GridSorting = $.ig.GridSorting || {};

	$.extend($.ig.GridSorting, {
		locale: {
			sortedColumnTooltipFormat: 'отсортировано ${direction}',
			unsortedColumnTooltip: 'нажмите для сортировки колонки',
			ascending: 'по возрастанию',
			descending: 'по убыванию',
			modalDialogSortByButtonText: 'сортировать',
			modalDialogResetButton: "сброс",
			modalDialogCaptionButtonDesc: "Нажмите для сортировки по убыванию",
			modalDialogCaptionButtonAsc: "Нажмите для сортировки по возрастанию",
			modalDialogCaptionButtonUnsort: "Нажмите для очистки сортировки",
			featureChooserText: "Порядок данных",
			modalDialogCaptionText: "Сортировать по нескольким",
			modalDialogButtonApplyText: 'Готово',
			modalDialogButtonCancelText: 'Отмена',
			sortingHiddenColumnNotSupport: 'Сортировка скрытых колонок не поддерживается',
			featureChooserSortAsc: 'Порядок А-Я',
			featureChooserSortDesc: 'Порядок Я-А'
			//modalDialogButtonSlideCaption: "Нажмите для показа/скрытия отсортированных колонок"
		}
	});

	$.ig.GridSummaries = $.ig.GridSummaries || {};

	$.extend($.ig.GridSummaries, {
		locale: {
			featureChooserText: "Скрыть сводки",
			featureChooserTextHide: "Показать сводки",
			dialogButtonOKText: 'OK',
			dialogButtonCancelText: 'Отмена',
			emptyCellText: '',
			summariesHeaderButtonTooltip: 'Показать/скрыть сводки',
			// M.H. 13 Oct. 2011 Fix for bug 91008
			defaultSummaryRowDisplayLabelCount: 'Кол',
			defaultSummaryRowDisplayLabelMin: 'Мин',
			defaultSummaryRowDisplayLabelMax: 'Макс',
			defaultSummaryRowDisplayLabelSum: 'Сумма',
			defaultSummaryRowDisplayLabelAvg: 'Сред',
			defaultSummaryRowDisplayLabelCustom: 'Вычисление',
			calculateSummaryColumnKeyNotSpecified: "Установите идентификатор поля (column key) для вычисления сводки",
			featureChooserNotReferenced: "Скрипт модуля выбора опций отсутствует. Чтобы избежать этой ошибки, добавьте ссылку на файл ig.ui.grid.featurechooser.js, используйте загрузчик или ссылку на комбинированный скрипт."
		}
	});

	$.ig.GridUpdating = $.ig.GridUpdating || {};

	$.extend($.ig.GridUpdating, {
		locale: {
			doneLabel: 'Готово',
			doneTooltip: 'Завершить редактирование и обновить',
			cancelLabel: 'Отмена',
			cancelTooltip: 'Завершить редактирование и не обновлять',
			addRowLabel: 'Добавить новую запись',
			addRowTooltip: 'Нажмите для редактирования новой записи',
			deleteRowLabel: '',
			deleteRowTooltip: 'Удалить запись',
			igEditorException: 'Редактирование ui.igGrid возможно только при наличии ui.igEditor',
			igComboException: 'Использование редактора спискового типа в ui.igGrid возможно только при наличии ui.igCombo',
			igRatingException: 'Использование редактора рейтингового типа в ui.igGrid возможно только при наличии ui.igRating',
			igValidatorException: 'Опции валидности в igGridUpdating доступны только при наличии ui.igValidator',
			noPrimaryKeyException: 'Обновление после удаления возможно только если установлено "primaryKey" в опциях igGrid.',
			hiddenColumnValidationException: 'Невозможно редактировать запись со скрытой колонкой и валидацией.',
			dataDirtyException: 'Таблица содержит несохраненные данные. Чтобы избежать сообщения об ошибке, установите опцию "autoCommit" в igGrid или обработайте событие "dataDirty" из igGridUpdating и возвратите false из обработчика. Обработчик также может вызвать "commit()" из igGrid.',
			recordOrPropertyNotFoundException: 'Не удалось найти указанную запись или свойство. Проверьте критерии поиска и, при необходимости, выполните их настройку.',
			rowEditDialogCaptionLabel: 'Редактировать запись',
			unboundColumnsNotSupported: 'Опция ColumnFixing не поддерживается с несвязанными столбцами',
			excelNavigationNotSupportedWithCurrentEditMode: 'Требуется другая конфигурация ExcelNavigation. Для editMode следует задать значение "cell" или "row".',
			undefinedCellValue: 'Undefined cannot be set as a cell value.'
		}
	});

    $.ig.ColumnMoving = $.ig.ColumnMoving || {};

    $.extend($.ig.ColumnMoving, {
        locale: {
            movingDialogButtonApplyText: 'Готово',
            movingDialogButtonCancelText: 'Отмена',
            movingDialogCaptionButtonDesc: 'Переместить вверх',
            movingDialogCaptionButtonAsc: 'Переместить вниз',
            movingDialogCaptionText: 'Переместить колонки',
            movingDialogDisplayText: 'Переместить колонки',
            movingDialogDropTooltipText: "Переместить сюда",
            dropDownMoveLeftText: 'Переместить влево',
            dropDownMoveRightText: 'Переместить вправо',
            dropDownMoveFirstText: 'Переместить в начало',
            dropDownMoveLastText: 'Переместить в конец',
            featureChooserNotReferenced: "Скрипт модуля выбора опций отсутствует. Чтобы избежать этой ошибки, добавьте ссылку на файл ig.ui.grid.featurechooser.js, используйте загрузчик или ссылку на комбинированный скрипт.",
            movingToolTipMove: 'Переместить',
            featureChooserSubmenuText: 'Двигать'
        }
    });

    $.ig.ColumnFixing = $.ig.ColumnFixing || {};

    $.extend($.ig.ColumnFixing, {
        locale: {
            headerFixButtonText: 'Нажмите для закрепления этой колонки',
            headerUnfixButtonText: 'Нажмите для открепления этой колонки',
            featureChooserTextFixedColumn: 'Закрепить колонку',
            featureChooserTextUnfixedColumn: 'Открепить колонку',
            groupByNotSupported: 'igGridGroupBy не поддерживается вместе с ColumnFixing',
            virtualizationNotSupported: 'Функция сетки – виртуализация позволяет выполнять виртуализацию как строк, так и столбцов. С опцией ColumnFixing виртуализация столбцов не поддерживается. Активируйте опцию rowVirtualization сетки',
            columnVirtualizationNotSupported: 'С опцией ColumnFixing виртуализация столбцов не поддерживается',
            columnMovingNotSupported: 'igGridColumnMoving не поддерживается вместе с ColumnFixing',
            hidingNotSupported: 'igGridHiding не поддерживается вместе с ColumnFixing',
            hierarchicalGridNotSupported: 'igHierarchicalGrid не поддерживается вместе с ColumnFixing',
            responsiveNotSupported: 'igGridResponsive не поддерживается вместе с ColumnFixing',
            noGridWidthNotSupported: 'При использовании опции ColumnFixing необходимо задавать ширину сетки в пикселах',
            defaultColumnWidthInPercentageNotSupported: "При использовании опции ColumnFixing ширина столбца в процентах по умолчанию не поддерживается",
            columnsWidthShouldBeSetInPixels: 'Для ColumnFixing требуются задать другую ширину столбца. Ширину столбца с ключом {key} следует задать в пикселях.',
            unboundColumnsNotSupported: 'Опция ColumnFixing не поддерживается с несвязанными столбцами',
            excelNavigationNotSupportedWithCurrentEditMode: "Режим навигации Excel поддерживается только для режимов редактирования ячейки и редактирования строки. Для предотвращения этой ошибки отключите опцию excelNavigationMode либо установите с помощью параметра editMode режим редактирования ячейки или строки.",
            initialFixingNotApplied: 'Исходная фиксация не может быть применена к столбцу с ключом: {0}. Причина: {1}', // {0} is placeholder for columnKey, {1} error message
            internalErrors: {
                none: 'Ошибки отсутствуют',
                notValidIdentifier: 'Столбец с указанным идентификатором отсутствует',
                fixingRefused: 'Фиксация невозможна, поскольку имеется ТОЛЬКО один видимый незафиксированный столбец',
                fixingRefusedMinVisibleAreaWidth: 'Фиксация столбца недопустима вследствие минимальной ширины видимой области для незафиксированных столбцов',
                alreadyHidden: 'Попытка зафиксировать / отменить фиксацию скрытого столбца',
                alreadyUnfixed: 'Попытка отменить фиксацию незафиксированного столбца',
                alreadyFixed: 'Попытка зафиксировать ранее зафиксированный столбец',
				unfixingRefused: 'Отмена фиксации невозможна, поскольку имеется только один видимый зафиксированный столбец и, по крайней мере, один скрытый зафиксированный столбец.',
				targetNotFound: 'Не удалось найти целевой столбец с ключом {key}. Проверьте ключ, который вы использовали для поиска и, при необходимости, выполните правку.'
            }
        }
    });

    $.ig.GridAppendRowsOnDemand = $.ig.GridAppendRowsOnDemand || {};

    $.extend($.ig.GridAppendRowsOnDemand, {
    	locale: {
    		loadMoreDataButtonText: 'Загрузить больше данных',
    		appendRowsOnDemandRequiresHeight: 'Требуется установить высоту для использования AppendRowsOnDemand',
    		groupByNotSupported: 'igGridGroupBy не поддерживается вместе с AppendRowsOnDemand',
    		pagingNotSupported: 'igGridPaging не поддерживается вместе с AppendRowsOnDemand',
    		cellMergingNotSupported: 'igGridCellMerging не поддерживается вместе с AppendRowsOnDemand',
    		virtualizationNotSupported: 'Виртуализация не поддерживается вместе с AppendRowsOnDemand'
    	}
    });

    $.ig.igGridResponsive = $.ig.igGridResponsive || {};

    $.extend($.ig.igGridResponsive, {
    	locale: {
    	    fixedVirualizationNotSupported: 'Опция igGridResponsive не поддерживается с фиксированной виртуализацией'
    	}
    });

    $.ig.igGridMultiColumnHeaders = $.ig.igGridMultiColumnHeaders || {};

    $.extend($.ig.igGridMultiColumnHeaders, {
    	locale: {
    	    multiColumnHeadersNotSupportedWithColumnVirtualization: 'Заголовки для нескольких столбцов не поддерживаются с опцией columnVirtualization'
    	}
    });

}
})(jQuery);