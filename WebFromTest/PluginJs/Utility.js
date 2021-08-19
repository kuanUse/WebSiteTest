/**
 * 使用POST傳遞參數進行檔案下載
 * @param {string} url (網址)
 * @param {Object} queryParams (參數)
 */
function U_downloadFilePost(url, queryParams) {
  // 避免舊有的iframe未刪除
  $("#download_iFrame").remove();
  // 加入iframe
  $("body").append(
    '<iframe name="downloadFrame" id="download_iFrame" style="display: none;" src="" />'
  );
  //建立form
  var myForm = document.createElement("form");
  myForm.method = "post";
  myForm.action = url;
  myForm.target = "download_iFrame";

  //輸入POST參數
  Object.getOwnPropertyNames(queryParams).forEach(function (
    ParamsName,
    idx,
    array
  ) {
    if (
      typeof queryParams[ParamsName] !== "undefined" &&
      queryParams[ParamsName] !== null
    ) {
      var myInput = document.createElement("input");
      myInput.setAttribute("name", ParamsName);
      myInput.setAttribute("value", queryParams[ParamsName]);
      myForm.appendChild(myInput);
    }
  });

  //結合frame至iframe
  $("#download_iFrame").append(myForm);
  //上傳參數
  myForm.submit();
  //移除ifame
  $("#download_iFrame").remove();
}

//產生unique ID
function U_UniqId() {
  return Math.round(new Date().getTime() + Math.floor(Math.random() * 1000));
}

//檢查變數是否為合法數字
function U_isNumber(value) {
  return isFinite(value);
}

//Json安全性檢查
function U_JsonSecurityChk(text) {
  return (
    !/[^,:{}\[\]0-9.\-+Eaeflnr-u \n\r\t]/.test(
      text.replace(/"(\\.|[^"\\])*"/g, "")
    ) && eval("(" + text + ")")
  );
}

//取得伺服器root路徑
function U_GetSiteRoot() {
  var rootPath = window.location.protocol + "//" + window.location.host + "/";
  if (window.location.hostname == "localhost") {
    var path = window.location.pathname;
    if (path.indexOf("/") == 0) {
      path = path.substring(1);
    }
    path = path.split("/", 1);
    if (path != "") {
      rootPath = rootPath + path + "/";
    }
  }
  return rootPath;
}

//取得網址參數
function U_GetUrlVars() {
  var vars = {},
    hash;
  var hashes = window.location.href
    .slice(window.location.href.indexOf("?") + 1)
    .split("&");
  for (var i = 0; i < hashes.length; i++) {
    hash = hashes[i].split("=");
    vars[hash[0]] = hash[1];
  }
  return vars;
}

//呼叫.cs的Webmethod
function U_AjaxCsMethod(URL, DataMap, SuccessFunc) {
  return $.ajax({
    type: "POST",
    url: URL,
    data: JSON.stringify(DataMap),
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: SuccessFunc
  });
}

//移除陣列重複值
function U_ArrayUnique(inputArray) {
  var result = [];
  $.each(inputArray, function (i, e) {
    if ($.inArray(e, result) == -1) result.push(e);
  });
  return result;
}

//轉換換行符號
function U_WrapSym(Value) {
  Value = Value.replace(new RegExp("&lt;", "gm"), "<");
  Value = Value.replace(new RegExp("&gt;", "gm"), ">");
  return Value;
}

function U_findObjectByKey(array, key, value) {
  for (var i = 0; i < array.length; i++) {
    if (array[i][key] == value) {
      return array[i];
    }
  }
  return null;
}

//檢查是否為字串及空值
function U_isNullOrEmpty(value) {
  if (value != null) {
    value = value.toString();
  }
  return !(typeof value === "string" && value.trim().length > 0);
}

//顯示bootstrap提示視窗
function U_showAlert(message, type, closeDelay) {
  var $cont = $("#alerts-container");

  if ($cont.length == 0) {
    // alerts-container does not exist, create it
    $cont = $('<div id="alerts-container">')
      .css({
        position: "fixed",
        width: "50%",
        left: "25%",
        top: "10%",
        "z-index": 1200
      })
      .appendTo($("body"));
  }

  // default to alert-info; other options include success, warning, danger
  type = type || "info";

  // create the alert div
  var alert = $("<div>")
    .addClass(" fade in show alert alert-" + type)
    .append(
      '<svg class="bi flex-shrink-0 me-2" width="24" height="24" role="img" aria-label="Danger:"><use xlink:href="#exclamation-triangle-fill"/></svg>'
    )
    .append(
      $('<button type="button" class="close" data-dismiss="alert">').append(
        "&times;"
      )
    )
    .append(message);

  // add the alert div to top of alerts-container, use append() to add to bottom
  $cont.prepend(alert);

  // if closeDelay was passed - set a timeout to close the alert
  if (closeDelay)
    window.setTimeout(function () {
      alert.alert("close");
    }, closeDelay);
}
//將屬性的值寫入至dom物件中
function U_LoadFormData($formObj, formData) {
  $.each($formObj.find(":input[name]"), function (i, e) {
    var fieldName = e.name;
    if (U_isNullOrEmpty(fieldName) || !formData.hasOwnProperty(fieldName)) {
      return;
    }
    var dom = $(e);
    if (dom.length == 0) {
      return;
    }
    var val = formData[fieldName];
    switch (e.type) {
      case "select-multiple":
      case "select-one":
        //若有使用selectpicker插件，則使用selectpicker的method清空內容
        if ($.isFunction($.fn.selectpicker)) {
          if (U_isNullOrEmpty(val)) {
            dom.val("").selectpicker("refresh");
            //移除X符號
            dom
              .closest("div.bootstrap-select")
              .find("div.bootstrap-select__clear")
              .remove();
          } else {
            dom.selectpicker("val", val);
          }
        } else {
          dom.val(val);
        }
        break;
      case "checkbox":
      case "radio":
        this.checked = false;
      case "file":
        return
      break;
      default:
        dom.val(val);
        break;
    }
  });
}
function U_ClearForm(selector) {
  selector.find(":input").each(function () {
    switch (this.type) {
      case "select-multiple":
      case "select-one":
        //若有使用selectpicker插件，則使用selectpicker的method清空內容
        if ($.isFunction($.fn.selectpicker)) {
          $(this)
            .val("")
            .selectpicker("refresh");
          //移除X符號
          $(this)
            .closest("div.bootstrap-select")
            .find("div.bootstrap-select__clear")
            .remove();
        } else {
          $(this).val("");
        }
        break;
      case "checkbox":
      case "radio":
        this.checked = false
      default:
        $(this).val("");
        break;;
    }
  });
}

/**
 * 顯示"進行中"視窗，防止使用者操作其他動作。
 */
function U_ShowPleaseWait(message) {
  if (U_isNullOrEmpty(message)) {
    message = "Please wait...";
  }
  if (document.querySelector("#pleaseWaitDialog") == null) {
    var modalLoading =
      '<div id="pleaseWaitDialog" class="modal" style="z-index: 1100" data-backdrop="static" data-keyboard="false" role="dialog">\
            <div class="modal-dialog">\
                <div class="modal-content">\
                    <div class="modal-header">\
                        <h4 class="modal-title">' +
      message +
      '</h4>\
                    </div>\
                    <div class="modal-body">\
                        <div class="progress">\
                          <div class="progress-bar progress-bar-success progress-bar-striped progress-bar-animated active" role="progressbar"\
                          aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width:100%; height: 40px">\
                          </div>\
                        </div>\
                    </div>\
                </div>\
            </div>\
        </div>';
    $(document.body).append(modalLoading);
  }
  $("#pleaseWaitDialog").modal("show");
}

/**
 * 關閉"進行中"視窗
 */
function U_HidePleaseWait() {
  $("#pleaseWaitDialog").modal("hide");
}
