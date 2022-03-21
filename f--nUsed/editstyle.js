(function($)
		{
			$.fn.vCenter = function(options) 
			{
				var pos = 
				{
					docHeight: function() { return Math.max(document.body.scrollHeight, document.documentElement.scrollHeight, document.body.offsetHeight, document.documentElement.offsetHeight, document.body.clientHeight, document.documentElement.clientHeight); },
					scrollTop: function() { return self.pageYOffset || (document.documentElement && document.documentElement.scrollTop) || (document.body && document.body.scrollTop); },
					scrollLeft: function() { return self.pageXOffset || (document.documentElement && document.documentElement.scrollLeft) || (document.body && document.body.scrollLeft);}
				};
				return this.each(function(index) 
				{
					if (index == 0) 
					{
						var $this = $(this);
						var elHeight = $this.height();
						var elWidth = $this.width();
						$this.css( {position:'absolute',marginTop:'0',top:pos.scrollTop()+50,display:'block'} );
					};
				});
			};	
		})(jQuery);

function trim12(str) 
{
	var	str = str.replace(/^\s\s*/, ''),
	ws = /\s/,
	i = str.length;
	while (ws.test(str.charAt(--i)));
	return str.slice(0, i + 1).replace(/"/g,"`").replace(/'/g,"`").replace(/&/g,"^");
}
		
// Change InState Select Background
function ChangeInState(obj)
{
	var myRoot = SelectRoot(obj,['line1','itm','InState']); 
	myRoot.setAttribute("class",'inState'+myRoot.value);
	SetMarker(obj,true,true);
}

// Change inputs for custom Lang
function ChangeLang(obj,toLang)
{
	var rus = obj.innerHTML == 'EN';			
	if(toLang !== undefined) rus = toLang == 'RU';
	var root = SelectRootPOI(obj);
	var subroot = SelectSubNode(root,'name_er');
	subroot = SelectSubNode(subroot,'itm');
	SelectSubNode(subroot,'txt').innerHTML = 'Имя ('+ (rus ? 'RU' : 'EN') +'):'
	SelectSubNode(subroot,'lang').innerHTML = rus ? 'RU' : 'EN';
	SelectSubNode(subroot,'Name_RU').style.display = rus ? 'inline' : 'none';
	SelectSubNode(subroot,'Name_EN').style.display = rus ? 'none' : 'inline';
	
	subroot = SelectSubNode(root,'hidden');
	subroot = SelectSubNode(subroot,'desc');
	SelectSubNode(subroot,'txt').innerHTML = 'Описание ('+ (rus ? 'RU' : 'EN') +'):'
	SelectSubNode(subroot,'lang').innerHTML = rus ? 'RU' : 'EN';
	SelectSubNode(subroot,'Desc_RU').style.display = rus ? 'inline' : 'none';
	SelectSubNode(subroot,'Desc_EN').style.display = rus ? 'none' : 'inline';
	
	var myRoot = SelectRoot(obj,['hidden','address','line1','itm']);
	SelectSubNode(myRoot,'txt3').innerHTML = 'Адрес, улица ('+ (rus ? 'RU' : 'EN') +'):'
	SelectSubNode(myRoot,'lang').innerHTML = rus ? 'RU' : 'EN';
	SelectSubNode(myRoot,'Street_RU').style.display = rus ? 'inline' : 'none';
	SelectSubNode(myRoot,'Street_EN').style.display = rus ? 'none' : 'inline';
	
	myRoot = SelectRoot(obj,['hidden','address','line3','itm']);
	SelectSubNode(myRoot,'txt3').innerHTML = 'город ('+ (rus ? 'RU' : 'EN') +'):'
	SelectSubNode(myRoot,'lang').innerHTML = rus ? 'RU' : 'EN';
	SelectSubNode(myRoot,'City_RU').style.display = rus ? 'inline' : 'none';
	SelectSubNode(myRoot,'City_EN').style.display = rus ? 'none' : 'inline';
	
	myRoot = SelectRoot(obj,['hidden','address','line5','itm']);
	SelectSubNode(myRoot,'txt3').innerHTML = 'дополнительно ('+ (rus ? 'RU' : 'EN') +'):'
	SelectSubNode(myRoot,'lang').innerHTML = rus ? 'RU' : 'EN';
	SelectSubNode(myRoot,'A_Ext_RU').style.display = rus ? 'inline' : 'none';
	SelectSubNode(myRoot,'A_Ext_EN').style.display = rus ? 'none' : 'inline';
	
	myRoot = SelectRoot(obj,['hidden','contacts','line5','itm']);
	SelectSubNode(myRoot,'txt3').innerHTML = 'дополнительно ('+ (rus ? 'RU' : 'EN') +'):'
	SelectSubNode(myRoot,'lang').innerHTML = rus ? 'RU' : 'EN';
	SelectSubNode(myRoot,'C_Ext_RU').style.display = rus ? 'inline' : 'none';
	SelectSubNode(myRoot,'C_Ext_EN').style.display = rus ? 'none' : 'inline';
}

// Find <div id="POI">
function SelectRootPOI(obj)
{
	var res = obj;
	while(res.id != "POI") res = res.parentNode;
	return res;
}

// Find SubNode by id="..."
function SelectSubNode(obj,id)
{
	for(var x = 0; x < obj.childNodes.length; x++)
		if(obj.childNodes[x].id == id) return obj.childNodes[x];
	return null;
}

// Find Full Node by id="..," with escalate to <div id="POI">
function SelectRoot(obj,ids)
{
	var res = SelectRootPOI(obj);
	for(var i=0;i<ids.length;i++) 
		res = SelectSubNode(res,ids[i]);
	return res;
}	

// collapse POI info
function more(obj,cls)
{
	var hObj = SelectRoot(obj,['hidden']);
	var vis = hObj.getAttribute("class") == "hid";
	if(cls !== undefined) vis = cls != "hid";
    hObj.setAttribute("class",vis ? "vis" : "hid");
	SelectRoot(obj,['bottom','itm','moreSel']).innerHTML = vis ? '&nbsp; кратко...&nbsp;' : '&nbsp;подробно...&nbsp;';
}
	// display icons text intered
function confirm_ext(obj)
{
	var mainRoot = SelectRoot(obj,['bottom','itm']);
	var ruR = SelectRoot(obj,['hidden','desc','Desc_RU']);
	var enR = SelectRoot(obj,['hidden','desc','Desc_EN']);
	var ex = (trim12(ruR.value).length > 0) || (trim12(enR.value).length > 0);			
	SelectSubNode(mainRoot,'exs_D').setAttribute("class",ex ? 'vis1' : 'vis0');
	
	var len = 0;
	if(trim12(SelectRoot(obj,['hidden','address','line1','itm','Street_RU']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','address','line1','itm','Street_EN']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','address','line2','itm','HouseNo']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','address','line3','itm','City_RU']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','address','line3','itm','City_EN']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','address','line4','itm','ZIP']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','address','line5','itm','A_Ext_RU']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','address','line5','itm','A_Ext_EN']).value).length > 0) len++;
	SelectSubNode(mainRoot,'exs_A').setAttribute("class",len > 0 ? 'vis1' : 'vis0');
	
	len = 0;
	if(trim12(SelectRoot(obj,['hidden','contacts','line1','itm','Phone']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','contacts','line2','itm','Fax']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','contacts','line3','itm','Email']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','contacts','line4','itm','Url']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','contacts','line5','itm','C_Ext_RU']).value).length > 0) len++;
	if(trim12(SelectRoot(obj,['hidden','contacts','line5','itm','C_Ext_EN']).value).length > 0) len++;
	SelectSubNode(mainRoot,'exs_C').setAttribute("class",len > 0 ? 'vis1' : 'vis0');
	
	len = trim12(SelectRoot(obj,['hidden','acomments','itm','OwnerComment']).value).length;
	SelectSubNode(mainRoot,'exs_my').setAttribute("class",len > 0 ? 'vis1' : 'vis0');
	
	len = trim12(SelectRoot(obj,['hidden','mtext','itm','ModeratorText']).value).length;
	SelectSubNode(mainRoot,'exs_mo').setAttribute("class",len > 0 ? 'vis1' : 'vis0');
}

// Check Lat/Lon isValid
function checkDouble(obj)
{
	obj.style.background = isNaN(parseFloat(obj.value)) ? '#FFCCCC' : '#CCFFCC';
}

function save(obj, multi) 
{ 		
	var mr = obj;
	var id = parseInt(trim12(SelectRoot(mr,['line1','itm','id']).value));
	if(isNaN(id)) id = -1;
	var str = '<?xml version="1.0" encoding="utf-8"?>\n<POIList>\n'
	str += '<POI>\n'+
		'<ID>'+id+'</ID>\n'+
		'<Region>'+trim12(SelectRoot(mr,['line1','itm','regID']).value)+'</Region>\n'+
		'<Name>\n'+
		  '<EN>'+trim12(SelectRoot(mr,['name_er','itm','Name_EN']).value)+'</EN>\n'+
		  '<RU>'+trim12(SelectRoot(mr,['name_er','itm','Name_RU']).value)+'</RU>\n'+
		'</Name>\n'+
		'<Lat>'+trim12(SelectRoot(mr,['line1','itm','Lat']).value)+'</Lat>\n'+
		'<Lon>'+trim12(SelectRoot(mr,['line1','itm','Lon']).value)+'</Lon>\n'+
		'<SymbolID>'+trim12(SelectRoot(mr,['hidden','itm','SymbolID']).value)+'</SymbolID>\n'+				
		'<Categories>'+trim12(SelectRoot(mr,['cats','itm2','CategoriesIDs']).value)+'</Categories>\n'+	
		'<Address>\n'+
			'<Street>\n'+
			'<EN>'+trim12(SelectRoot(mr,['hidden','address','line1','itm','Street_EN']).value)+'</EN>\n'+
			'<RU>'+trim12(SelectRoot(mr,['hidden','address','line1','itm','Street_RU']).value)+'</RU>\n'+
			'</Street>\n'+
			'<HouseNo>'+trim12(SelectRoot(mr,['hidden','address','line2','itm','HouseNo']).value)+'</HouseNo>\n'+
			'<City>\n'+
			'<EN>'+trim12(SelectRoot(mr,['hidden','address','line3','itm','City_EN']).value)+'</EN>\n'+
			'<RU>'+trim12(SelectRoot(mr,['hidden','address','line3','itm','City_RU']).value)+'</RU>\n'+
			'</City>\n'+
			'<ZIP>'+trim12(SelectRoot(mr,['hidden','address','line4','itm','ZIP']).value)+'</ZIP>\n'+
			'<Ext>\n'+
			'<EN>'+trim12(SelectRoot(mr,['hidden','address','line5','itm','A_Ext_EN']).value)+'</EN>\n'+
			'<RU>'+trim12(SelectRoot(mr,['hidden','address','line5','itm','A_Ext_RU']).value)+'</RU>\n'+
			'</Ext>\n'+
		'</Address>\n'+
		'<Contacts>\n'+
			'<Phone>'+trim12(SelectRoot(mr,['hidden','contacts','line1','itm','Phone']).value)+'</Phone>\n'+
			'<Fax>'+trim12(SelectRoot(mr,['hidden','contacts','line2','itm','Fax']).value)+'</Fax>\n'+
			'<Email>'+trim12(SelectRoot(mr,['hidden','contacts','line3','itm','Email']).value)+'</Email>\n'+
			'<Ext>\n'+
			 '<EN>'+trim12(SelectRoot(mr,['hidden','contacts','line5','itm','C_Ext_EN']).value)+'</EN>\n'+
			 '<RU>'+trim12(SelectRoot(mr,['hidden','contacts','line5','itm','C_Ext_RU']).value)+'</RU>\n'+
			'</Ext>\n'+
			'<URL>'+trim12(SelectRoot(mr,['hidden','contacts','line4','itm','Url']).value)+'</URL>\n'+
		'</Contacts>\n'+
		'<Description>\n'+
		  '<EN>'+trim12(SelectRoot(mr,['hidden','desc','Desc_EN']).value)+'</EN>\n'+
		  '<RU>'+trim12(SelectRoot(mr,['hidden','desc','Desc_RU']).value)+'</RU>\n'+
		'</Description>\n'+
		'<Owner>'+trim12(SelectRoot(mr,['bottom','itm','Owner']).value)+'</Owner>\n'+
		'<Expires>'+trim12(SelectRoot(mr,['name_er','itm','Expires']).value)+'</Expires>\n'+
		'<OwnerComment>'+trim12(SelectRoot(mr,['hidden','acomments','itm','OwnerComment']).value)+'</OwnerComment>\n'+
		'<CreatedBy>'+trim12(SelectRoot(mr,['hidden','itm','CreatedBy']).value)+'</CreatedBy>\n'+
		'<InState>'+trim12(SelectRoot(mr,['line1','itm','InState']).value)+'</InState>\n'+
		'<ModeratorText>'+trim12(SelectRoot(mr,['hidden','mtext','itm','ModeratorText']).value)+'</ModeratorText>\n';
		try
		{
			str += '<ModeratorComment>'+trim12(SelectRoot(mr,['hidden','mcomment','itm2','ModeratorComment']).value)+'</ModeratorComment>\n';
		} catch (e) {};
		str += '</POI>\n';			
		str += '</POIList>';
	
	SelectRoot(obj,['bottom','itm','saveText','moreSel']).setAttribute("class",'vis0');
	$(SelectRootPOI(obj)).css({ opacity: 0.25 });
	makeRPCSend("GetForm.aspx?set=new",str,ret_postPOI_data,[[obj],multi]);
}		

var ret_postPOI_data = function(objs, status, receivedText) 
{
	for(var i = 0;i<objs[0].length;i++) 
	{
		$(SelectRootPOI(objs[0][i])).css({ opacity: 1 });
		SelectRoot(objs[0][i],['bottom','itm','saveText','moreSel']).setAttribute("class",'vis1');	
	};
	if(status == 200)
	{		
		var xml = parseXml(receivedText);
		var alText = "";
		if($(xml).find('status POIList error').length > 0)
		{
			$(xml).find('status POIList error').each(function(index,value)
			{ 
				var txt = $(this).text();
				if(txt.length > 0)
					alText += txt +"\n";
			});
			alert('Не удается сохранить данные!\n'+alText);
		}
		else
		{					
			$(xml).find('status POI').each(function(index,value)
			{ 
				var obj = objs[0][index];
				var letter = SelectRoot(obj,['cats','itm2','mStyle','mLetter']).innerHTML;
				if($(this).find('error').length > 0)
				{
					$(xml).find('error').each(function(index,value)
					{ 
						var txt = $(this).text();
						if(txt.length > 0)
							alText += txt +"\n";
					});
					var id  = parseInt($(this).find('ID').text());
					if(isNaN(id)) id = -1;
					if(id > 0)
						AddToLog('Не удалось сохранить информацию о POI '+letter+' #'+id+' на сервере!','red')
					else
						AddToLog('Не удалось добавить информацию о POI '+letter+' на сервер!','red');
					alert('Не удается сохранить информацию по точке: '+letter+'\n'+alText);
				}
				else // ok
				{
					var id  = $(this).find('ID').text();
					SelectRoot(obj,['line1','itm','id']).value = id;
					//alert('Ok: '+$(this).find('ID').text());
					AddToLog('Информация о POI '+letter+' #'+id+' сохранена на сервере.','green');
					reload(obj,objs[1]);
				};
			});			
		};
	};
}

function parseXml(str) {
  if (window.ActiveXObject) {
    var doc = new ActiveXObject('Microsoft.XMLDOM');
    doc.loadXML(str);
    return doc;
  } else if (window.DOMParser) {
    return (new DOMParser).parseFromString(str, 'text/xml');
  } else
    return "";
}
		
function sCat(obj) 
{ 
	var letter = SelectRoot(obj,['cats','itm2','mStyle','mLetter']).innerHTML;
	var id = SelectRoot(obj,['line1','itm','id']).value;
	if(isNaN(id)) id = -1;
	if(id > 0) letter += " #" + id;
	var categories = SelectRoot(obj,['cats','itm2','CategoriesIDs']).value;
	ShowSelectCategoriesDialog(letter, categories, obj);
}		
function sReg(obj)
{
	var letter = SelectRoot(obj,['cats','itm2','mStyle','mLetter']).innerHTML;
	var id = SelectRoot(obj,['line1','itm','id']).value;
	if(isNaN(id)) id = -1;
	if(id > 0) letter += " #" + id;
	var region = parseInt(SelectRoot(obj,['line1','itm','regID']).value);
	if(isNaN(region)) region = -1;
	ShowSelectRegionDialog(letter, region, obj);
}
function reload(obj,multi) 
{ 	
	var root = SelectRootPOI(obj);
	$(root).css({ opacity: 0.25 });
	SelectRoot(root,['bottom','itm','reloadText','moreSel']).setAttribute("class",'vis0');
	var id = SelectRoot(root,['line1','itm','id']).value;
	if(isNaN(id)) id = -1;
	$.get('GetForm.aspx?get=id&id='+id, function(data) 
	{
		DeleteMarker(root);
		root.innerHTML = $(data)[0].innerHTML;
		$('div#POIList div#POI b#mLetter').each(function(index,value)
		{ 
			if(value.innerHTML == 0) value.innerHTML = GetAvailableLetter();
		});
		$(root).css({ opacity: 1 });
		SetMarker(root,true,multi);
		mapMarkerObj = root;
		SelectRoot(root,['bottom','itm','mrkSet']).checked = true;
		var letter = SelectRoot(root,['cats','itm2','mStyle','mLetter']).innerHTML;
		if(id > 0)
		{
			AddToLog('Информация о POI '+letter +' #'+id+' обновлена с сервера.','navy')
			AddToLog('Клик на карте установит координаты для POI '+letter +' #'+id+'.','orange');
		}
		else
		{
			AddToLog('Информация о POI '+letter +' обнулена.','navy');
			AddToLog('Клик на карте установит координаты для POI '+letter +'.','orange');
		};			
	});
}	

function reloadall()
{
	if(!confirm('Вы действительно хотите обновить все точки?\nЭто сбросит все ваши несохраненные изменения точек!')) return;
	$('div#POIList div#POI').each(function(index,value)
	{
		var id = parseInt(trim12(SelectRoot(value,['line1','itm','id']).value));
		if(isNaN(id)) id = -1;		
		if(id > 0) reload(value,true);
	});
}

function AddNew()
{	
	$('#POIAdd')[0].innerHTML = 'Подождите, идет загрузка...';
	$.get('GetForm.aspx?get=empty', function(data) 
	{
		$('#POIAdd').remove();
		$('#POIList').append(data);		
		var ttl = 0;
		$('div#POIList div#POI b#mLetter').each(function(index,value)
		{ 
			ttl++;
			if(value.innerHTML == 0) value.innerHTML = GetAvailableLetter();
		});
		mapMarkerObj = $('div#POIList div#POI:last')[0];		
		$('div#POIList div#POI input#mrkSet:last')[0].checked = true;
		if(ttl < 25)
			$('#POIList').append('<div id="POIAdd"><a href="#" onclick="AddNew();return false;">+ Добавить еще...</a></div>');
		var letter = SelectRoot(mapMarkerObj,['cats','itm2','mStyle','mLetter']).innerHTML;
		AddToLog('Добавлена форма для ввода информации о POI '+letter+'.','gray');
		AddToLog('Клик на карте установит координаты для POI '+letter +'.','orange');
	});	
}

function saveall()
{
	if(!confirm('Вы действительно хотите сохранить все точки?\nЭто может сбросить статус для уже промодерированных точек!')) return;
	$('div#POIList div#POI').each(function(index,value)
	{
		save(value,true);
	});
}

function AddToLog(line,color)
{
	var currentTime = new Date();
	var hours = currentTime.getHours();
	var minutes = currentTime.getMinutes();
	if (minutes < 10) minutes = "0" + minutes;
	var seconds = currentTime.getSeconds();
	if (seconds < 10) seconds = "0" + seconds;
	
	var c = 'white';
	if(color !== undefined) c = color;
	$('div#logDiv')[0].innerHTML = hours + ":" + minutes + ":" + seconds + ' <span style="color:'+c+';">' + line + '</span><br/>' + $('div#logDiv')[0].innerHTML;
}

//                 //                       //                 //                       //                 //                       //                 //                       //                 //                       
var ShowSelectRegionDialog = function(caption,rid,obj)
{
	var dlg = null;
	if($('#SelectRegion').length == 0)
	{
		$('body').append('<div id="SelectRegion"><div id="SelectDialogHeader"><b>&nbsp;<span id="hccapt">Регион</span> POI <span id="SelectRegionCaption">&nbsp;</span></b></div>&nbsp;<br/><div align="center">'+
		'<select id="regionListSel"></select></div>'+
		'&nbsp;<br/><div align="center"><input type="button" value="Ок" style="border:solid 1px gray;background:silver;width:90px;" onclick="DoneSelectRegion();"/>&nbsp; &nbsp;<input type="button" value="Отмена" style="border:solid 1px gray;background:silver;width:90px;" onclick="CancelSelectRegion();"/></div></div>');
		for(var i=0;i<RegionList.length;i++)		
			$('select#regionListSel')[0].options[i] = new Option(RegionList[i].Name,RegionList[i].ID);
	};
	$('#SelectRegionCaption')[0].innerHTML = caption;
	dlg = $('#SelectRegion').vCenter();
	dlg[0].obj = obj;
	$('div#bodydiv').css({ opacity: 0.35 });
	if(rid < 1)
		$('select#regionListSel')[0].value = RegionList[0].ID;
	else
		$('select#regionListSel')[0].value = rid;
}

var DoneSelectRegion = function()
{
	var obj = $('#SelectRegion')[0].obj;
	var region = $('select#regionListSel')[0].value;
	SelectRoot(obj,['line1','itm','regID']).value = region;
	var regName = '';
	for(var i=0;i<RegionList.length;i++)		
		if(region == RegionList[i].ID)
			regName = RegionList[i].Name;
	SelectRoot(obj,['line1','itm','region']).value = regName;
	CancelSelectRegion();
}

var CancelSelectRegion = function()
{
	$('#SelectRegion').hide();
	$('div#bodydiv').css({ opacity: 1 });
}

var ShowSelectCategoriesDialog = function(caption,categories,obj)
{
	var dlg = null;
	if($('#SelectCategories').length == 0)
	{
		$('body').append('<div id="SelectCategories"><div id="SelectDialogHeader"><b>&nbsp;<span id="hccapt">Категории</span> POI <span id="SelectCategoriesCaption">&nbsp;</span></b></div>&nbsp;<br/><div align="center">'+
		'<div id="SelCatList"></div>'+
		'</div>'+
		'&nbsp;<br/><div align="center"><input type="button" value="Ок" style="border:solid 1px gray;background:silver;width:90px;" onclick="DoneCategoriesRegion();"/>&nbsp; &nbsp;<input type="button" value="Отмена" style="border:solid 1px gray;background:silver;width:90px;" onclick="CancelCategoriesRegion();"/></div></div>');
		for(var i=0;i<CategoryList.length;i++)
			$('div#SelCatList')[0].innerHTML += '<input type="checkbox" id="catItm" name="catItm_'+i+'"/>'+
			'<a href="#" onclick="toggleCat('+i+');return false;">'+CategoryList[i].RU+' ('+CategoryList[i].EN+')</a>'+
			'<br/>';
		
	};
	$('#SelectCategoriesCaption')[0].innerHTML = caption;
	
	for(var x=0;x<CategoryList.length;x++)
		$("input#catItm[name='catItm_"+x+"']")[0].checked = false;
	
	var cats = categories.split(",");
	for(var i = 0;i<cats.length;i++)
	{
		var cat = trim12(cats[i]);
		for(var x=0;x<CategoryList.length;x++)
			if(CategoryList[x].EN == cat) 
				$("input#catItm[name='catItm_"+x+"']")[0].checked = true;
	};
	
	dlg = $('#SelectCategories').vCenter();
	dlg[0].obj = obj;
	$('div#bodydiv').css({ opacity: 0.35 });
}

var toggleCat = function(ind)
{
	var obj = $("input#catItm[name='catItm_"+ind+"']")[0];
	obj.checked = !obj.checked;
}

var DoneCategoriesRegion = function()
{
	var obj = $('#SelectCategories')[0].obj;
	var categories = '';
	var categoriesRU = '';
	$("input#catItm").each(function(index,value)
	{
		if(value.checked) 
		{
			if(categories.length > 0) { categories += ","; categoriesRU += ", "; };
			categories += CategoryList[index].EN;
			categoriesRU += CategoryList[index].RU;
		};
	});
	SelectRoot(obj,['cats','itm2','CategoriesIDs']).value = categories;
	SelectRoot(obj,['cats','itm2','Categories']).value = categoriesRU.length > 0 ? categoriesRU : 'Не выбраны';
	CancelCategoriesRegion();
}

var CancelCategoriesRegion = function()
{
	$('#SelectCategories').hide();
	$('div#bodydiv').css({ opacity: 1 });
}