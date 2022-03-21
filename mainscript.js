var global_vars_array = [/* 0 - onDoc Ready Load JS */[],/* onDoc Ready Load CSS */ []];
var parseUrl = null;
var map = null;
var geocoder = null;

		// загрузка файлов JS
		// -1 - on document ready; false/0 - now; 1-99999 - timeOut
		function include(path, _to)
		{
			if(!(_to))
				document.write('<script type="text/javascript"  src="'+path+'" charset="utf-8"></script>');
			else if(_to == -1)
				global_vars_array[0].push(path);
			else 
				setTimeout("LoadJS('"+path+"');",_to);
		}
		
		// загрузка файлов СSS
		// -1 - on document ready; false/0 - now; 1-99999 - timeOut
		function includeCSS(path, _to)
		{
			if(!(_to))
				document.write('<link type="text/css" rel="stylesheet" href="'+path+'"/>');
			else if(_to == -1)
				global_vars_array[1].push(path);
			else 
				setTimeout("LoadCSS('"+path+"');",_to);
		}
		
		// загрузка файлов JS и CSS после прогрузки страницы
		function includeDone()
		{
			for(var i=0;i<global_vars_array[0].length;i++) LoadJS(global_vars_array[0][i]);				
			global_vars_array[0] = [];
			for(var i=0;i<global_vars_array[1].length;i++) LoadCSS(global_vars_array[1][i]);
			global_vars_array[1] = [];			
			
			if(mapInit) mapInit();
			if(pageOnload) pageOnload();
		}
		
		// загрузка файлов JS и CSS после прогрузки страницы
		// document.onready = function() { setTimeout('includeDone();',2000); };  //  works only w/jQuery	
		document.dkxce_onload = function() { setTimeout('includeDone();',2500); };
		if (document.addEventListener) { document.addEventListener( "DOMContentLoaded", function() //Mozilla, Opera
		{ document.removeEventListener( "DOMContentLoaded", arguments.callee, false ); document.dkxce_onload(); }, false ); }
		else if (document.attachEvent) { document.attachEvent("onreadystatechange", function() { if ( document.readyState === "complete" )  // IE
		{ document.detachEvent( "onreadystatechange", arguments.callee ); document.dkxce_onload(); }; }) }
		else window.onload = function() { includeDone(); }; // always works

include('xml_xslt.js'); // LoadJS, LoadCSS, HTTPReq Library
include('error_cache.js'); // Errors Log Library
include('jQuery.js'); // jQuery Library
include('raphael.js'); // Raphael Graphics Library
include('parseurl.js'); // ParseURL Library
include('GMap.js'); // Google Maps Scripts
include('Rotation.js'); // Marker Direction Icon Library

	var pageOnload = (function () 
	{
		var testMarker = new MapAlert(new GLatLng(55.45, 37.39), 0, 0);	
		map.addOverlay(testMarker);
		var gm = new GMarker(new GLatLng(55.45, 37.39), {draggable: false});
		map.addOverlay(gm);
		AddDirectionMarker(testMarker,0,function(angle){$('div#angle:first')[0].innerHTML = angle;});	
		testMarker.setAngle(320);
	
		var testMarker2 = new MapAlert(new GLatLng(55.47, 38.01), 0, 0);	
		map.addOverlay(testMarker2);
		var gm2 = new GMarker(new GLatLng(55.47, 38.01), {draggable: false});
		map.addOverlay(gm2);
		AddDirectionMarker(testMarker2,0,function(angle){$('div#angle:first')[0].innerHTML = angle;});	
	});