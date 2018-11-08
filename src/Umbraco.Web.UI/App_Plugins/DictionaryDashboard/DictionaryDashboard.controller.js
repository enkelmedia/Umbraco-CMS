angular.module("umbraco").controller("Obviuse.DictionaryDashboardController",
function ($http, angularHelper, dictionaryService, $timeout) {

	/* --------------------- Construction ------------------- */

	var vm = this;
	vm.loading = true;
	vm.languages = [];
	vm.tempEditCollection = [];
	vm.search = {
		key: '',
		onlyShowMatches: false,
		search : false
	};
	vm.state = {
		showLanguageMenu: false,
		listLimit : 50
	};

	/* --------------------- Event Handlers ------------------- */

	vm.languageFilterChanged = function (lang) {

		// clear the key-filter
		vm.search.key = '';

		// clear out other filters
		angular.forEach(vm.languages, function (item, value) {
			if (item.name !== lang.name) {
				item.filter = "";
			}
		});

		vm.applyFilters();

	};

	vm.showMatchesToggle = function () {
		vm.applyFilters();
		vm.showHideMaches();
		vm.setupOddAndEvenRows();
	};

	vm.setFilterKeyText = function () {

		vm.clearDictionaryValueFilters();
		vm.applyFilters();

	};

	vm.enterEditMode = function (row, index) {

		if (row.editMode) {
			vm.closeAllEditModes();
			return;
		}

		vm.closeAllEditModes();

		vm.tempEditCollection = [];
		// Copy each translation to temp-edit-collection
		angular.forEach(vm.languages, function (value, key) {
			var item = value.dictionaries[index];
			vm.tempEditCollection.push(
				{
					key: item.key,
					value: item.value,
					index: item.index,
					id: item.id
				});
		});

		row.editMode = true;
	};

	vm.cancelEdit = function (row) {
		vm.closeAllEditModes();
	};

	vm.hideLanguage = function (lang) {
		lang.hidden = true;
		vm.state.showLanguageMenu = vm.showLanguageMenu();
	};

	vm.showLanguage = function (lang) {
		lang.hidden = false;
		vm.state.showLanguageMenu = vm.showLanguageMenu();
	};

	vm.keypress = function ($event, row) {
		// Is this the enterkey aka, key 13 ?
		if ($event.charCode === 13) {
			vm.save(row);
			$event.preventDefault();
			$event.stopPropagation();
		}
	};

	vm.save = function (row) {

		row.editMode = false;

		var saveDics = {
			id: 0,
			translations: []
		};

		// Copy over data from the tempModel to the "real" model for each lang
		angular.forEach(vm.languages, function (lang, key) {

			var temp = vm.tempEditCollection[key];

			lang.dictionaries[row.index] = {
				key: temp.key,
				value: temp.value,
				index: temp.index,
				id: row.id
			};

			saveDics.id = temp.id;
			saveDics.key = temp.key;
			saveDics.translations.push({
				languageId: lang.languageId,
				languageCode: lang.code,
				value: temp.value
			});

		});

		// Save to backend
		dictionaryService.saveItem(saveDics).then(function (response) { });

		// We nee to loop over the collection of visible dictionaries to 
		// check matches for the one with the current row.index
		for (var i = 0; i < vm.dictionaries.length; i++) {
			if (vm.dictionaries[i].index === row.index) {
				vm.dictionaries[i].isMatch = vm.lookForMatch(row.index);
			}
		}

		// Clear out temp-collection
		vm.tempEditCollection = [];

	};

	vm.clearAllFilters = function () {

		vm.clearDictionaryValueFilters();

		vm.search.key = '';
		vm.search.onlyShowMatches = false;

		vm.applyFilters();
	};

	/* --------------------- Private Methods ------------------- */

	vm.makeAllDictionariesVisible = function() {
		for (var i = 0; i < vm.dictionaries.length; i++) {
			vm.dictionaries[i].visible = true;
		}
	};

	vm.getAllDictionaries = function() {
		var res = [];

		for (var i = 0; i < vm.languages[0].dictionaries.length; i++) {
			var dic = vm.languages[0].dictionaries[i];
			dic.index = i;
			dic.visible = true; // always visible when loading.
			dic.isOdd = mathHelper.getEvenOrOdd();
			res.push(dic);
		}
			
		return res;
	};

	vm.applyFilters = function () {

		vm.search.hasFilters = true;
		if (vm.search.key !== '') {

			var query = vm.search.key.toLowerCase();
			// Filter dics based on the key, use the first lang-array and fiter from this.
			for (var i = 0; i < vm.dictionaries.length; i++) {
				if (vm.dictionaries[i].key.toLowerCase().indexOf(query) !== -1) {
					vm.dictionaries[i].visible = true;
				}
				else {
					vm.dictionaries[i].visible = false;
				}
			}
			vm.prepareDictionariesForRendering();
			return;

		}
		else {
			for (var i = 0; i < vm.languages.length; i++) {
				if (vm.languages[i].filter !== undefined && vm.languages[i].filter !== '') {

					vm.filterForDictionaryValue(vm.languages[i]);
					vm.prepareDictionariesForRendering();

					return;
				}
			}
		}

		// At this state, there are no other filters than possibly the checkbox,
		// thats why the checkbox value will determan the state of the "hasFilters"-prop
		vm.search.hasFilters = vm.search.onlyShowMatches;

		// If we come this far there are no filter applyed
		vm.makeAllDictionariesVisible();
		vm.prepareDictionariesForRendering();
	};

	vm.prepareDictionariesForRendering = function() {
		vm.showHideMaches();
		vm.setupOddAndEvenRows();
	};

	vm.filterForDictionaryValue = function (lang) {

		var filterQuery = lang.filter.toLowerCase();
		if (filterQuery === '') {
			vm.makeAllDictionariesVisible();
			return;
		}

		var orgLang = vm.languages[lang.index];
			
		for (var i = 0; i < vm.dictionaries.length; i++) {
			// if match is found, add to tempDics
			if (orgLang.dictionaries[i].value.toLowerCase().indexOf(filterQuery) !== -1)
			{
				vm.dictionaries[i].visible = true;
			}
			else
			{
				vm.dictionaries[i].visible = false;
			}
		}
	};

	vm.setupOddAndEvenRows = function() {

		mathHelper.reset();

		for (var i = 0; i < vm.dictionaries.length; i++) {
			if (vm.dictionaries[i].visible === true) {
				var val = mathHelper.getEvenOrOdd();
				console.log();
				vm.dictionaries[i].isOdd = val;
			}
		}

	};

	vm.showHideMaches = function() {

		// if the checkbox is not checked we don't need to care.
		if (vm.search.onlyShowMatches === true) {
			for (var i = 0; i < vm.dictionaries.length; i++) {
				if (vm.dictionaries[i].visible === true) {
					vm.dictionaries[i].visible = vm.dictionaries[i].isMatch;
				}
			}
		}
	};

	vm.closeAllEditModes = function() {
		// close all other edits
		angular.forEach(vm.dictionaries,
			function(value, key) {
				value.editMode = false;
			});
	};

	vm.lookForMatch = function (dictionaryIndex) {

		// add all values to an array
		var arrValues = [];

		angular.forEach(vm.languages, function(lang, key) {
				arrValues.push(
					{
						name: lang.name,
						value: lang.dictionaries[dictionaryIndex].value
					}
				);
			});

		// compare all values against each other to find out any matches.
		for (var i = 0; i < arrValues.length; i++) {
			var currentValue = arrValues[i];

			for (var j = 0; j < arrValues.length; j++) {
				// don't compare the same object
				if (i !== j) {
					if (currentValue.value === arrValues[j].value) {
							
						return true;
					}
				}
			}
		}
			
		return false;
	};

	vm.setupMatches = function() {
		// Loop over all dicanaries update match-value for each dic.
		for (var i = 0; i < vm.dictionaries.length; i++) {
			vm.dictionaries[i].isMatch = vm.lookForMatch(vm.dictionaries[i].index);
		}
	};

	vm.showLanguageMenu = function() {

		for (var i = 0; i < vm.languages.length; i++) {
			if (vm.languages[i].hidden) {
				return true;
			}
		}
		return false;
	};

		
	vm.setTableHeight = function() {
		// Set table height
        var wH = $('#contentcolumn').height();
		$('.dictionary-table-wrapper').css({ height: (wH - 85) });

		// Setting up eventhandlers
		window.onresize = function (event) {
			vm.setTableHeight();
		};
	};

	

	vm.clearDictionaryValueFilters = function () {

		for (var i = 0; i < vm.languages.length; i++) {
			vm.languages[i].filter = '';
		}

	};

		
	vm.init = function () {
		dictionaryService.getAll().then(function (response) {

			// adding index på lang-objects
			var res = response.data;
			for (var i = 0; i < res.length; i++) {
				res[i].index = i;
			}

			vm.languages = res;
			vm.dictionaries = vm.getAllDictionaries();
			vm.setupMatches();

			vm.loading = false;

			/*
			 // This was a test to speed up init-load, keeping for reference
			var tempColl = [];
			for (var j = 0; j < res.length; j++) {

				var curr = {
					code: res[j].code,
					dictionaries: [],
					index: res[j].index,
					languageId: res[j].languageId,
					name: res[j].name
				};


				console.log(curr);
				var tempDics = [];
				for (var k = 0; k < 20; k++) {
					tempDics.push({
							id: res[j].dictionaries[k].id,
							index: res[j].dictionaries[k].index,
							isMatch: res[j].dictionaries[k].isMatch,
							isOdd: res[j].dictionaries[k].isOdd,
							key: res[j].dictionaries[k].key,
							value: res[j].dictionaries[k].value,
							visible: res[j].dictionaries[k].visible
						}
					);
				}
				//curr.dictionaries[k]);;
				curr.dictionaries = tempDics;
				tempColl.push(curr);
				
			}

			vm.languages = tempColl;
			vm.loading = false;

			// setup
			vm.dictionaries = vm.getAllDictionaries();
			vm.setupMatches();

			$timeout(function () {

				console.log(res);
				// setting up all the dics
				vm.languages = res;
				vm.dictionaries = vm.getAllDictionaries();
				vm.setupMatches();

			}, 1500);
			*/


			vm.setTableHeight();

			document.addEventListener("keydown", function (e) {
				// 27 = esc-key
				if (e.keyCode === 27) {
					vm.closeAllEditModes();
				}
			}, false);

			//console.log('setiup');
			//var topofDiv = $(".dictionary-table-wrapper").offset().top; //gets offset of header
			//var height = $(".dictionary-table-wrapper").outerHeight(); //gets height of header

			//$('.dictionary-table-wrapper').on('mousewheel DOMMouseScroll', function (e) {

			//	console.log(e);
			//	var top = $('.dictionary-table-wrapper').scrollTop();
			//	var height = $('.dictionary-table-wrapper').outerHeight();
			//	var offset = $('.dictionary-table-wrapper').offset().top;

			//	console.log('top', top);
			//	console.log('height', height);
			//	console.log('offset', offset);

			//	console.log('elemen scroll');

			//	if ($(window).scrollTop() > (topofDiv + height)) {
			//		console.log('scroll: true');
			//	}
			//	else {
			//		console.log('scroll: false');
			//	}

			//});

			//$(document).scroll(function () {
			//	console.log('scrolling..');
			//});

			//$(window).scroll(function () {
			//	console.log('scrolling');
			//	if ($(window).scrollTop() > (topofDiv + height)) {
			//		console.log('scroll: true');
			//	}
			//	else {
			//		console.log('scroll: false');
			//	}
			//});

		});
	};

	vm.log = function(p1, p2, p3, p4, p5, p6) {
		//console.log(p1, p2, p3, p4, p5, p6);
	};

	vm.init();

});

var mathHelper = {

	lastEvenOrOddValue: true,
	reset : function () {
		mathHelper.lastEvenOrOddValue = true;
	},
	getEvenOrOdd : function () {
		mathHelper.lastEvenOrOddValue = !mathHelper.lastEvenOrOddValue;
		return mathHelper.lastEvenOrOddValue;
	},
	isOdd(num) {
		return num % 2; 
	}
};

angular.module("umbraco.resources").factory("dictionaryService", function ($http) {

	var service = {};
	service.getAll = function (id) {
		return $http.get("Backoffice/Obviuse/DictionaryDashboard/GetAll");
	};

    service.saveItem = function(row) {
		return $http.post("Backoffice/Obviuse/DictionaryDashboard/Save", row);
    };

    return service;
});

/*
// Increase the "limit" of the ng-repeat for the table when scrolling towards the bottom
var timer;
var scrollReloadLock = false;
function scrolled(o) {
	
	var currentScrollPos = o.offsetHeight + o.scrollTop;
	var scrollHeight = o.scrollHeight - 300;

	//console.log('currentScrollPos', o.offsetHeight + o.scrollTop);
	//console.log('scrollHeight', o.scrollHeight);

	if (scrollReloadLock === false) {
		if (currentScrollPos > (scrollHeight - 350)) {
			
			var scope = angular.element($(".dictionary-dashboard-wrapper")).scope();
			scope.$apply(function () {

				if (scope.vm.languages[0].dictionaries.length > scope.vm.state.listLimit) {
					scope.vm.state.listLimit = scope.vm.state.listLimit + 20;
					scrollReloadLock = true;
				}
			});

			setTimeout(function () {
				scrollReloadLock = false;
			}, 10);

		}
		

	}

}
*/
console.log('glenn');
