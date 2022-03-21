var baseIcon = new GIcon(G_DEFAULT_ICON);
baseIcon.shadow = "http://www.google.com/mapfiles/shadow50.png";
baseIcon.iconSize = new GSize(20,34);
baseIcon.shadowSize = new GSize(37,34);
baseIcon.iconAnchor = new GPoint(9,34);
baseIcon.infoWindowAnchor = new GPoint(9,2);

parseUrl = new ParseURL();

var mapInitialize = function() 
{
	if (GBrowserIsCompatible()) 
	{
        map = new GMap2(document.getElementById("map_canvas"));        
		var lat = 55;
		lon = 38;
		zoom = 5;
		if((parseUrl.getHashParam('lat')) && (parseUrl.getHashParam('lon'))) 
		{
			lat = parseFloat(parseUrl.getHashParam('lat'));
			lon = parseFloat(parseUrl.getHashParam('lon'));
			zoom = parseUrl.getHashParam('z') ? parseInt(parseUrl.getHashParam('z')) : map.getZoom();			
		};
		map.setCenter(new GLatLng(lat, lon), zoom);
        geocoder = new GClientGeocoder();
                
        // OpenStreetMap Copyright
		var copycol = new GCopyrightCollection("");		
        var copy = new GCopyright(1, new GLatLngBounds(new GLatLng(-90,-180),new GLatLng(90,180)), 0, "&copy;2010 <a href='http://www.openstreetmap.org' style='font-size:10px;' target='_blank'>OpenStreetMap</a> contributors");
        copycol.addCopyright(copy);				
		
		// OpenStreetMap (Mapnik Render)
		var tileMapnik = new GTileLayer(copycol,1,18);
        tileMapnik.myBaseURL = "http://tile.openstreetmap.org/";
		tileMapnik.getTileUrl = function (a,b,c) { return this.myBaseURL + "/" + b + "/" + a.x + "/" + a.y + ".png";};
		tileMapnik.isPng = function () { return true;}
        tileMapnik.getOpacity = function() {return 1.0;}
        var layer0 = [tileMapnik];
        var mapnikMap = new GMapType(layer0, G_SATELLITE_MAP.getProjection(), "OSM-M", {alt:'Карта проекта OpenStreetMap (Mapnik Render)'});
        map.addMapType(mapnikMap);
        map.addMapType(G_HYBRID_MAP);
		map.setMapType(mapnikMap); // Set Default Map Type
		
		// OpenStreetMap (MapSurfer Render)
		var MapSurfer = new GTileLayer(copycol,1,18);
        MapSurfer.myBaseURL = "http://tiles1.mapsurfer.net/tms_r.ashx?";
		MapSurfer.getTileUrl = function (a,b,c) { return this.myBaseURL + "z=" + b + "&x=" + a.x + "&y=" + a.y;};
		MapSurfer.isPng = function () { return true;}
        MapSurfer.getOpacity = function() {return 1.0;}
        var layer0 = [MapSurfer];
        var MapSurferMap = new GMapType(layer0, G_SATELLITE_MAP.getProjection(), "OSM-MS", {alt:'Карта проекта OpenStreetMap (MapSurfer Render)'});
        map.addMapType(MapSurferMap);
		
		// OpenStreetMap (MapSurfer Topo Render)
		var MapSurferTopo = new GTileLayer(copycol,1,18);
        MapSurferTopo.myBaseURL = "http://tiles2.mapsurfer.net/tms_t.ashx?";
		MapSurferTopo.getTileUrl = function (a,b,c) { return this.myBaseURL + "z=" + b + "&x=" + a.x + "&y=" + a.y;};
		MapSurferTopo.isPng = function () { return true;}
        MapSurferTopo.getOpacity = function() {return 1.0;}
        var layer0 = [MapSurferTopo];
        var MapSurferMapTopo = new GMapType(layer0, G_SATELLITE_MAP.getProjection(), "OSM-MST", {alt:'Карта проекта OpenStreetMap (MapSurfer Topo Render)'});
        map.addMapType(MapSurferMapTopo);      
		
		// OpenStreetMap (Opnvkarte Render)
		var Opnvkarte = new GTileLayer(copycol,1,18);
        Opnvkarte.myBaseURL = "http://tile.xn--pnvkarte-m4a.de/tilegen";
		Opnvkarte.getTileUrl = function (a,b,c) { return this.myBaseURL + "/" + b + "/" + a.x + "/" + a.y + ".png";};
		Opnvkarte.isPng = function () { return true;}
        Opnvkarte.getOpacity = function() {return 1.0;}
        var layer0 = [Opnvkarte];
        var OpnvkarteMap = new GMapType(layer0, G_SATELLITE_MAP.getProjection(), "OSM-OV", {alt:'Карта проекта OpenStreetMap (Opnvkarte Render)'});
        map.addMapType(OpnvkarteMap);
		
		// Navitel Copyright
		var copycol = new GCopyrightCollection("");		
        var copynav = new GCopyright(1, new GLatLngBounds(new GLatLng(-90,-180),new GLatLng(90,180)), 0, "&copy;2010 <a href='http://www.navitel.su' style='font-size:10px;' target='_blank'>Navitel</a>");
        copycol.addCopyright(copynav);	
		
		// Navitel
		var Navitel = new GTileLayer(copycol,1,18);
        Navitel.myBaseURL = "http://maps.navitel.su/navitms.fcgi?t=";
		Navitel.getTileUrl = function (a,b,c) 
		{ 
			var xx = a.x.toString();
			while(xx.length < 8) xx = '0'+xx;
			var zz = b.toString();
			while(zz.length < 2) zz = '0'+zz;
			var yy = Math.round(Math.pow(2,b)-a.y)-1;
			yy = yy.toString();
			while(yy.length < 8) yy = '0'+yy;
			return this.myBaseURL + xx + ',' + yy + ',' + zz;
		};
		Navitel.isPng = function () { return true;}
        Navitel.getOpacity = function() {return 1.0;}
        var layer0 = [Navitel];
        var NavitelMap = new GMapType(layer0, G_SATELLITE_MAP.getProjection(), "Navitel", {alt:'Карта Navitel'});
        map.addMapType(NavitelMap);
		
		// Wikimapia Copyright
		var copycol = new GCopyrightCollection("");		
        var copyWikimapia = new GCopyright(1, new GLatLngBounds(new GLatLng(-90,-180),new GLatLng(90,180)), 0, "&copy;2010 <a href='http://www.wikimapia.orf' style='font-size:10px;' target='_blank'>Wikimapia</a>");
        copycol.addCopyright(copyWikimapia);	
		
		// Wikimapia
		var Wikimapia = new GTileLayer(copycol,1,18);
        Wikimapia.myBaseURL = ["http://i",".wikimapia.org/?lng=1&"];		
		Wikimapia.getTileUrl = function (a,b,c)  {  return this.myBaseURL[0] + ((a.x % 4) + (a.y % 4)*4) + this.myBaseURL[1] + 'x=' + a.x + '&y='+a.y+'&zoom='+b; };
		Wikimapia.isPng = function () { return true;}
        Wikimapia.getOpacity = function() {return 1.0;}
        var layer0 = [Wikimapia];
        var WikimapiaMap = new GMapType(layer0, G_SATELLITE_MAP.getProjection(), "Wikimapia", {alt:'Карта Wikimapia'});
        map.addMapType(WikimapiaMap);
				
		// Генштаб Copyright
		var copycolG = new GCopyrightCollection("");		
        var copyG = new GCopyright(1, new GLatLngBounds(new GLatLng(-90,-180),new GLatLng(90,180)), 0, "&copy;2010 <a href='http://www.in7ane.com/' style='font-size:10px;' target='_blank'>Генштаб</a>");
        copycolG.addCopyright(copyG);	
		
		// Генштаб
	    var tileIN7 = new GTileLayer(copycolG,1,18);
        tileIN7.myBaseURL = "http://www.in7ane.com/topomaps/tiles/";
		tileIN7.getTileUrl = function (xy,z,c) { return this.myBaseURL + z + '/'+xy.x+'/'+xy.y+'.jpg';  };
		tileIN7.isPng = function () { return false; }
        tileIN7.getOpacity = function() {return 1.0;}
        var layer1 = [tileIN7];
        var IN7Map = new GMapType(layer1, G_SATELLITE_MAP.getProjection(), "Генштаб", {alt:'Сканы Генштабовских карт с in7ane.com'});
        map.addMapType(IN7Map);   
                
        
        // Рельеф Maps For Free Copyright
		var copycolMFF = new GCopyrightCollection("");		
        var copyMFF = new GCopyright(1, new GLatLngBounds(new GLatLng(-90,-180),new GLatLng(90,180)), 0, "&copy;2010 <a href='http://www.maps-for-free.com/' style='font-size:10px;' target='_blank'>Maps for Free</a>");
        copycolMFF.addCopyright(copyMFF);	
        
        // Рельеф Maps For Free
	    var tileMFF = new GTileLayer(copycolMFF,1,18);
        tileMFF.myBaseURL = "http://www.maps-for-free.com/layer/relief/";
		tileMFF.getTileUrl = function (xy,z,c) { return this.myBaseURL + '/z' + z + '/row'+xy.y+'/'+z+'_'+xy.x+'-'+xy.y+'.jpg';  };
		tileMFF.isPng = function () { return false; }
        tileMFF.getOpacity = function() {return 1.0;}
        var layer2 = [tileMFF];
        var MFFMap = new GMapType(layer2, G_SATELLITE_MAP.getProjection(), "Maps4Free", {alt:'Карта рельефа с проекта Maps-for-Free'});
        map.addMapType(MFFMap);   
       
        // Yahoo Copyright
		var copycolYH1 = new GCopyrightCollection("");		
        var copyYH1 = new GCopyright(1, new GLatLngBounds(new GLatLng(-90,-180),new GLatLng(90,180)), 0, "&copy;2010 <a href='http://maps.yahoo.com/' style='font-size:10px;' target='_blank'>Yahoo! Maps</a>");
        copycolYH1.addCopyright(copyYH1);	
        
        // Yahoo Maps
	    var tileYH1 = new GTileLayer(copycolYH1,1,18);
        tileYH1.myBaseURL = "http://us.maps1.yimg.com/us.tile.maps.yimg.com/tl?v=4.1&md=2&r=1";
		tileYH1.getTileUrl = function (xy,z,c) { return this.myBaseURL + '&x='+xy.x+'&y='+(((Math.round(Math.pow(2,z))/2)-1)-xy.y)+'&z='+(z+1);  };
		tileYH1.isPng = function () { return true;}
        tileYH1.getOpacity = function() {return 1.0;}
        var layer3 = [tileYH1];
        var YH1Map = new GMapType(layer3, G_SATELLITE_MAP.getProjection(), "Yahoo!", {alt:'Карта Yahoo!'});
        map.addMapType(YH1Map);  
        
        // Yahoo Sat
	    var tileYH2 = new GTileLayer(copycolYH1,1,18);
        tileYH2.myBaseURL = "http://aerial.maps.yimg.com/ximg?v=1.8&t=a&s=256&r=1";
		tileYH2.getTileUrl = function (xy,z,c) { return this.myBaseURL + '&x='+xy.x+'&y='+(((Math.round(Math.pow(2,z))/2)-1)-xy.y)+'&z='+(z+1);  };
		tileYH2.isPng = function () { return true;}
        tileYH2.getOpacity = function() {return 1.0;}
        var layer4 = [tileYH2];
        var YH2Map = new GMapType(layer4, G_SATELLITE_MAP.getProjection(), "Yahoo!Sat", {alt:'Спутниковые снимки Yahoo!'});
        map.addMapType(YH2Map);  
        
        // Yahoo Hyb
	    var tileYH3 = new GTileLayer(copycolYH1,1,18);
        tileYH3.myBaseURL = "http://aerial.maps.yimg.com/ximg?v=2.5&t=p&s=256&r=1";
		tileYH3.getTileUrl = function (xy,z,c) { return this.myBaseURL + '&x='+xy.x+'&y='+(((Math.round(Math.pow(2,z))/2)-1)-xy.y)+'&z='+(z+1);  };
		tileYH3.isPng = function () { return true;}
        tileYH3.getOpacity = function() {return 1.0;}
        var layer5 = [tileYH2,tileYH3];
        var YH3Map = new GMapType(layer5, G_SATELLITE_MAP.getProjection(), "Yahoo!Hyb", {alt:'Спутниковая карта Yahoo! с нанесенными названиями улиц'});
        map.addMapType(YH3Map);  
		
		// Microsoft
		var getMsnTileUrl = function(a, b, mapType) {
			var imageSuffix;
			var mapTypeString;
			var mapTilesVersion = 22;
			switch (mapType) {
				case G_NORMAL_MAP:
					imageSuffix = ".png";
					mapTypeString = "r";
					break;
				case G_SATELLITE_MAP:
					imageSuffix = ".jpeg";
					mapTypeString = "a";
					break;
				case G_HYBRID_MAP:
					imageSuffix = ".jpeg";
					mapTypeString = "h";
					break;
			};			
			var sTile = '000000';
			sTile += (parseInt(a.y.toString(2) * 2) + parseInt(a.x.toString(2)));
			sTile = sTile.substring(sTile.length - b, sTile.length);
			return 'http://' + mapTypeString + sTile.substring(sTile.length-1, sTile.length) + '.ortho.tiles.virtualearth.net/tiles/' + mapTypeString + sTile + imageSuffix + '?g=' + mapTilesVersion;
		}
    
		var msnCopyrights = new GCopyrightCollection();
		var year = (new Date()).getFullYear();
		msnCopyrights.addCopyright(new GCopyright(1, new GLatLngBounds(new GLatLng(-90,-180),new GLatLng(90,180)), 0, "&copy;" + year + " Microsoft Corporation  &copy;" + year + " NAVTEQ"));

		var msnRoadLayer = new GTileLayer(msnCopyrights, 1, 16);
		msnRoadLayer.getTileUrl = function(a, b) { return getMsnTileUrl(a, b, G_NORMAL_MAP); }
	
		var msnSatelliteLayer = new GTileLayer(msnCopyrights, 1, 16);
		msnSatelliteLayer.getTileUrl = function(a, b) { return getMsnTileUrl(a, b, G_SATELLITE_MAP); };
	
		var msnHybridLayer = new GTileLayer(msnCopyrights, 1, 16);
		msnHybridLayer.getTileUrl = function(a, b) { return getMsnTileUrl(a, b, G_HYBRID_MAP); };
	
		var msnRoadMapType = new GMapType([msnRoadLayer], G_NORMAL_MAP.getProjection(), 'MS Map', {});
		var msnSatelliteMapType = new GMapType([msnSatelliteLayer], G_SATELLITE_MAP.getProjection(), 'MS Sat', {});
		var msnHybridMapType = new GMapType([msnHybridLayer], G_HYBRID_MAP.getProjection(), 'MS Hyb', {});
	
		map.addMapType(msnRoadMapType);
		map.addMapType(msnSatelliteMapType); 
		map.addMapType(msnHybridMapType); 
	    
		/*
			var mapControl = new GHierarchicalMapTypeControl();
			mapControl.clearRelationships();
			mapControl.addRelationship(YH2Map, YH3Map, "Показать ярлыки", false);
			map.addControl(mapControl);
			
			var mapControlMS = new GHierarchicalMapTypeControl();
			mapControlMS.clearRelationships();
			mapControlMS.addRelationship(msnSatelliteMapType, msnHybridMapType, "Показать ярлыки", false);
			map.addControl(mapControlMS);
		*/
		
		map.addControl(new GMenuMapTypeControl());
        map.addControl(new GOverviewMapControl());
        map.addControl(new GLargeMapControl3D());
        map.addControl(new GScaleControl());
        map.enableScrollWheelZoom();
			
		
		GEvent.addListener(map, "moveend", function() {
			//			
			parseUrl.setHashParam('lat',map.getCenter().lat());
			parseUrl.setHashParam('lon',map.getCenter().lng());
			parseUrl.setHashParam('z',map.getZoom());
			parseUrl.SetLoc();
		}); 
    };                        
}
	       
//POI No
function MapAlert(point, id, InState) 
{	    
      this.point_ = point;
      this.id_ = id;
      this.InState_ = InState;
    }
    MapAlert.prototype = new GOverlay();
    MapAlert.prototype.initialize = function(map) {
	  var d2 = document.createElement("div");
	  d2.style.position = "absolute";
	  d2.style.width = "40px";
	  d2.style.height = "70px";
	  d2.style.background = "url('dirBg.png') no-repeat";
      map.getPane(G_MAP_MAP_PANE).appendChild(d2);
      var div = document.createElement("div");
	  div.style.cursor = "default";
      div.style.position = "absolute";
      div.style.width = "32px";
	  div.style.height = "32px";
      map.getPane(G_MAP_MAP_PANE).appendChild(div);
      this.map_ = map;
	  this.d2_ = d2;
      this.div_ = div;      	  	  
    }
    MapAlert.prototype.remove = function() {
      this.div_.parentNode.removeChild(this.div_);
	  this.div_.parentNode.removeChild(this.d2_);
    }
    MapAlert.prototype.copy = function() {
      return new MapAlert(this.point_, this.id_, this.InState_);
    }
	MapAlert.prototype.move = function(glatlng)
	{
		this.point_ = glatlng;
		this.redraw(true);
	}
    MapAlert.prototype.redraw = function(force) {      
      if (!force) return; // We only need to redraw if the coordinate system has changed
      var xy = this.map_.fromLatLngToDivPixel(this.point_);
      this.div_.style.left = (xy.x - 16) + "px";
      this.div_.style.top = (xy.y + 14) + "px";   
	  this.d2_.style.left = (xy.x - 20) + "px";
      this.d2_.style.top = (xy.y - 21) + "px"; 
}
   
	
//		//		//		//		//		//		//		//		//		//		
		
var mapInit = function()
{
	// Init Map
	mapInitialize(); 		
}	   