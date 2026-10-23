/* ============================================================
   Bullows PaintBooth Pro — paintbooth.js
   All calculations ported from old PaintBooth page
   Global functions defined OUTSIDE $(function(){})
   ============================================================ */

/* ─────────────────────────────────────────────────────────────
   GLOBAL STATE
───────────────────────────────────────────────────────────── */
var standardPanelSize = 1140;
var standardPanelHeight = 2390;
var isManualHChange = false;
var selectedMotorHP = 0;
var ductRowIndex = 1;
var currentEnquiryCategory = '';

/* ─────────────────────────────────────────────────────────────
   3D DUCTING PREVIEW
───────────────────────────────────────────────────────────── */
var ductScene, ductCamera, ductRenderer, ductPivot, ductGroup, ductInited = false;
var ductRotX = 0, ductRotY = 0, ductDragging = false, ductLastX = 0, ductLastY = 0, ductZoom = 1;

function initDuctThreeD() {
    if (ductInited) return;
    var wrap = document.getElementById('ductThreeWrap');
    if (!wrap) return;

    ductScene = new THREE.Scene();
    ductCamera = new THREE.PerspectiveCamera(45, wrap.clientWidth / wrap.clientHeight, 0.1, 1000);
    ductRenderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    ductRenderer.setPixelRatio(window.devicePixelRatio || 1);
    ductRenderer.setSize(wrap.clientWidth, wrap.clientHeight);
    wrap.appendChild(ductRenderer.domElement);

    ductScene.add(new THREE.AmbientLight(0xffffff, 0.7));
    var dl = new THREE.DirectionalLight(0xffffff, 0.8);
    dl.position.set(3, 5, 4);
    ductScene.add(dl);

    ductPivot = new THREE.Group();
    ductScene.add(ductPivot);

    // drag to rotate
    var el = ductRenderer.domElement;
    el.addEventListener('mousedown', function (e) { ductDragging = true; ductLastX = e.clientX; ductLastY = e.clientY; wrap.style.cursor = 'grabbing'; });
    window.addEventListener('mouseup', function () { ductDragging = false; wrap.style.cursor = 'grab'; });
    window.addEventListener('mousemove', function (e) {
        if (!ductDragging) return;
        ductRotY += (e.clientX - ductLastX) * 0.01;
        ductRotX += (e.clientY - ductLastY) * 0.01;
        ductLastX = e.clientX; ductLastY = e.clientY;
    });
    el.addEventListener('wheel', function (e) {
        e.preventDefault();
        ductZoom *= (e.deltaY > 0 ? 1.1 : 0.9);
        ductZoom = Math.max(0.4, Math.min(3, ductZoom));
    }, { passive: false });

    window.addEventListener('resize', function () {
        if (!wrap.clientWidth) return;
        ductCamera.aspect = wrap.clientWidth / wrap.clientHeight;
        ductCamera.updateProjectionMatrix();
        ductRenderer.setSize(wrap.clientWidth, wrap.clientHeight);
    });

    ductInited = true;
    (function animate() {
        requestAnimationFrame(animate);
        ductPivot.rotation.x = ductRotX;
        ductPivot.rotation.y = ductRotY;
        ductRenderer.render(ductScene, ductCamera);
    })();
}

function renderDuctThreeD() {
    initDuctThreeD();
    if (!ductInited) return;

    if (ductGroup) { ductPivot.remove(ductGroup); }
    ductGroup = new THREE.Group();
    ductPivot.add(ductGroup);

    // all directions in the XY plane (top / plan view)
    var DIR = { up: [0, 1, 0], down: [0, -1, 0], right: [1, 0, 0], left: [-1, 0, 0] };
    var COL = { duct: 0x9aa4b2, bend: 0xD85A30 };

    // cross-section from the actual duct fields (mm → scene units)
    var wMM = parseFloat($('#txtExhaustWidth').val()) || 500;
    var hMM = parseFloat($('#txtexhaustheight').val()) || 500;
    var SC = 1 / 2000;                 // mm → units
    var CW = Math.max(0.15, wMM * SC); // duct width  → in-plane face
    var CD = Math.max(0.15, hMM * SC); // duct height → depth (Z)

    function mat(c) { return new THREE.MeshStandardMaterial({ color: c, metalness: 0.25, roughness: 0.5 }); }

    function ductSeg(p1, p2, color) {
        var d = new THREE.Vector3().subVectors(p2, p1);
        var len = d.length();
        if (len < 0.001) return;
        var m = new THREE.Mesh(new THREE.BoxGeometry(CW, len, CD), mat(color));
        m.position.copy(p1).add(p2).multiplyScalar(0.5);
        m.quaternion.setFromUnitVectors(new THREE.Vector3(0, 1, 0), d.clone().normalize());
        ductGroup.add(m);
    }

    // swept 90° rectangular elbow from dirIn → dirOut, returns end position
    function elbow(p, dirIn, dirOut, color) {
        if (Math.abs(dirIn.dot(dirOut)) > 0.99) { return p.clone(); }
        var R = Math.max(CW, CD) * 1.1;
        var C = p.clone().add(dirOut.clone().multiplyScalar(R));
        var N = 8, prev = p.clone();
        for (var i = 1; i <= N; i++) {
            var t = (Math.PI / 2) * (i / N);
            var pt = C.clone()
                .add(dirIn.clone().multiplyScalar(Math.sin(t) * R))
                .add(dirOut.clone().multiplyScalar(-Math.cos(t) * R));
            ductSeg(prev, pt, color);
            prev = pt;
        }
        return prev;
    }

    var pos = new THREE.Vector3(0, 0, 0);
    var dir = new THREE.Vector3(0, 1, 0);

    $('#componentsTable tbody tr[data-duct]').each(function () {
        var type = $(this).find('.duct-type').text().trim();
        if (type === 'Duct') {
            var orient = ($(this).data('orient') || '').toString().toLowerCase();
            if (orient === 'vertical') dir = new THREE.Vector3(0, 1, 0);
            else if (orient === 'horizontal') dir = new THREE.Vector3(1, 0, 0);
            var mm = parseInt($(this).data('len')) || 0;
            var len = Math.max(0.4, mm * SC);      // real length, same mm→units scale
            var next = pos.clone().add(dir.clone().multiplyScalar(len));
            ductSeg(pos, next, COL.duct);
            pos = next;
        } else {
            var dd = ($(this).data('dir') || 'up').toString().toLowerCase();
            var dirOut = new THREE.Vector3().fromArray(DIR[dd] || DIR.up);
            pos = elbow(pos, dir.clone(), dirOut, COL.bend);
            dir = dirOut;
        }
    });

    // fit camera to the XY extents
    var box = new THREE.Box3().setFromObject(ductGroup);
    if (box.isEmpty()) box.set(new THREE.Vector3(-0.3, -0.3, -0.3), new THREE.Vector3(0.3, 0.3, 0.3));
    var center = box.getCenter(new THREE.Vector3());
    var size = box.getSize(new THREE.Vector3());
    ductGroup.position.copy(center).multiplyScalar(-1);
    var maxDim = Math.max(size.x, size.y, 1);
    var dist = (maxDim / (2 * Math.tan(Math.PI * 45 / 360))) * 1.4 * ductZoom;
    ductCamera.position.set(0, 0, dist);
    ductCamera.up.set(0, 1, 0);
    ductCamera.lookAt(0, 0, 0);
}

/* ─────────────────────────────────────────────────────────────
   DUCTING — ROWS, WEIGHTS, HIDDEN FIELD
───────────────────────────────────────────────────────────── */
function ductRowNetWeight(type, lenMM) {
    var W = parseFloat($('#txtExhaustWidth').val()) || 0;
    var H = parseFloat($('#txtexhaustheight').val()) || 0;
    var thk = parseFloat($('#txtExhaustThickness').val()) || 0;
    var perimeter = 2 * (W + H);
    var density = 7.85;                                  // kg per m² per mm

    var runMM = (type === 'Duct') ? lenMM : (Math.PI / 2) * W;   // 90° elbow arc, R ≈ W
    var areaM2 = (perimeter * runMM) / 1e6;
    return Math.round(areaM2 * thk * density);           // net — no wastage
}

function ductRowGrossWeight(type, lenMM) {
    return Math.round(ductRowNetWeight(type, lenMM) * 1.10);   // +10% wastage
}

function buildDuctRowHtml(idx, type, lenMM, dir, orient, netWt, grossWt) {
    var isDuct = type === 'Duct';
    var display = isDuct
        ? (orient ? lenMM + ' - ' + orient : lenMM)      // 500 - Vertical
        : (dir || '-');

    return '<tr data-duct="1"' +
        ' data-dir="' + (isDuct ? '' : (dir || '')) + '"' +
        ' data-orient="' + (isDuct ? (orient || '') : '') + '"' +
        ' data-len="' + (isDuct ? (lenMM || 0) : 0) + '">' +
        '<td>' + idx + '</td>' +
        '<td class="duct-type">' + type + '</td>' +
        '<td class="duct-len">' + display + '</td>' +
        '<td class="duct-net" style="text-align:right;">' + (netWt || 0) + '</td>' +
        '<td class="duct-wt" style="text-align:right;">' + (grossWt || 0) + '</td>' +
        '<td style="text-align:right;"><button type="button" class="setting-tbl-edit me-1 duct-edit"><i class="bi bi-pencil"></i> Edit</button>' +
        '<button type="button" class="setting-tbl-remove duct-del"><i class="bi bi-trash3"></i></button></td></tr>';
}

function updateDuctWeightTotal() {
    var net = 0, gross = 0;
    $('#componentsTable tbody tr[data-duct]').each(function () {
        net += parseInt($(this).find('.duct-net').text()) || 0;
        gross += parseInt($(this).find('.duct-wt').text()) || 0;
    });
    $('#txtDuctNetWeight').val(net);
    $('#txtDuctGrossWeight').val(gross);
    $('#ductNetTotalCell').text(net);
    $('#ductWtTotalCell').text(gross);
}

// Duct rows store their orientation (Vertical / Horizontal) in BendDirection
function updateDuctHiddenField() {
    var list = [];
    $('#componentsTable tbody tr[data-duct]').each(function () {
        var type = $(this).find('.duct-type').text().trim();
        list.push({
            BendType: type,
            BendDirection: type === 'Bend'
                ? ($(this).data('dir') || null)
                : ($(this).data('orient') || null),
            DuctLength: type === 'Duct' ? (parseInt($(this).data('len')) || null) : null,
            DuctNetWeight: parseFloat($(this).find('.duct-net').text()) || 0,
            DuctGrossWeight: parseFloat($(this).find('.duct-wt').text()) || 0
        });
    });
    $('#hiddenDuctingList').val(JSON.stringify(list));
}

// Recalculate every row from current width / height / thickness,
// then refresh totals and the hidden JSON that gets saved
function recalcAllDuctWeights() {
    $('#componentsTable tbody tr[data-duct]').each(function () {
        var type = $(this).find('.duct-type').text().trim();
        var len = parseInt($(this).data('len')) || 0;
        $(this).find('.duct-net').text(ductRowNetWeight(type, len));
        $(this).find('.duct-wt').text(ductRowGrossWeight(type, len));
    });
    updateDuctWeightTotal();
    updateDuctHiddenField();
}

function updateDuctRowNumbers() {
    $('#componentsTable tbody tr[data-duct]').each(function (i) { $(this).find('td:first').text(i + 1); });
    ductRowIndex = $('#componentsTable tbody tr[data-duct]').length + 1;
}

/* ─────────────────────────────────────────────────────────────
   DIMENSION CALCULATIONS
───────────────────────────────────────────────────────────── */
function calculateD(subTypeOfDraftType) {
    var D1 = parseFloat($('#txtD1').val()) || 0;
    var D2 = parseFloat($('#txtD2').val()) || 0;
    var Depth = parseFloat($('#txtlenght').val()) || 0;
    $('#txtD').val(D1 + D2 + Depth);
    calculatePanels(standardPanelSize);
    calculateCrossSectionalArea(subTypeOfDraftType);
}

function calculateW(subTypeOfDraftType) {
    var W1 = parseFloat($('#txtw1').val()) || 0;
    var W2 = parseFloat($('#txtw2').val()) || 0;
    var Width = parseFloat($('#txtwidth').val()) || 0;
    $('#txtw').val(W1 + W2 + Width);
    updateTxtW3();
    calculatePanelsforW(standardPanelHeight);
    calculateCrossSectionalArea(subTypeOfDraftType);
    calculateCapacity();
    calculateHingedDoorWidth();
}

function calculateH(subTypeOfDraftType) {
    if (!isManualHChange) {
        var H1 = parseFloat($('#txtH1').val()) || 0;
        var H2 = parseFloat($('#txtH2').val()) || 0;
        var Height = parseFloat($('#txtheight').val()) || 0;
        var PH = H1 + H2 + Height;
        $('#txtH').val(PH);
        $('#txtHingedDoorHeight').val(PH - 200);
    }
    calculateCrossSectionalArea(subTypeOfDraftType);
    calculateCapacity();
    calculatePanelsforH(standardPanelHeight);
    calculatePanelHeightAboveFilterFrame();
    validateDoorDimensions();
}

function updateTxtW3() {
    var W = parseFloat($('#txtw').val()) || 0;
    $('#txtw3').val(Math.round(W / 2));
}

function calculateCrossSectionalArea(subType) {
    var W = parseFloat($('#txtw').val()) || 0;
    var H = parseFloat($('#txtH').val()) || 0;
    var D = parseFloat($('#txtD').val()) || 0;
    var area = Math.round((W / 1000) * (H / 1000));
    var areaForSemiDDraught = Math.round((W / 1000) * (D / 1000));
    if (subType == "8" || subType == "9") {
        $('#txtCrossSectionalArea').val(areaForSemiDDraught);
    } else {
        $('#txtCrossSectionalArea').val(area);
    }
}

function calculateCapacity() {
    var area = parseFloat($('#txtCrossSectionalArea').val()) || 0;
    var velocity = parseFloat($('#txtVelocityofBlower').val()) || 0;
    var mps = area * velocity;
    var mph = mps * 3600;
    $('#txtCapacityofBlower').val(Math.round(mps));
    $('#txtCapacityofBlowerinH').val(Math.round(mph));
    $('#txtCapacityofBlowerinHReadonly').val(Math.round(mph));
    calculateFilterFrame();
}

function calculatePanels(panelSize) {
    var D = parseFloat($('#txtD').val()) || 0;
    $('#txtstandardPanels').val(Math.floor(D / panelSize));
    $('#txtremaingPanels').val(Math.round(D % panelSize));
}

function calculatePanelsforW(panelHeight) {
    var W = parseFloat($('#txtw').val()) || 0;
    var standardPanels = Math.floor(W / panelHeight);
    $('#txtstandardPanelsByW').val(standardPanels);
    $('#txtremaingPanelsByW').val(Math.round(W - (standardPanels * panelHeight)));
}

function calculatePanelsforH(panelHeight) {
    var H = parseFloat($('#txtH').val()) || 0;
    var standardPanels = Math.floor(H / panelHeight);
    var remaining = H - (standardPanels * panelHeight);
    $('#txtstandardPanelsByH').val(standardPanels);
    $('#txtremaingPanelsByH').val(Math.round(remaining));
    $('#remainingHeightError').toggle(remaining > 0 && remaining < 250);
}

function calculatePanelHeightAboveFilterFrame() {
    var H = parseFloat($('#txtH').val()) || 0;
    var EC = parseFloat($('#txtExtractionChamberHeight').val()) || 0;
    var ph = H - EC;
    $('#txtPanelHeightAboveFilterFrame').val(ph.toFixed(2));

    var $err = $('#panelHeightAboveFilterError');
    var $msg = $('#panelHeightAboveFilterMsg');

    if (ph > 0 && ph < 250) {
        $msg.html('Panel height above filter frame is <strong>' + ph.toFixed(0) + ' mm</strong>, which is too small. Minimum 250 mm required. Increase booth height H to at least <strong>' + (EC + 250) + ' mm</strong>.');
        $err.show();
        $('#submitbtn').prop('disabled', true);
        $('#SubmitError').show();
    } else if (ph <= 0 && H > 0) {
        var chamberLbl = currentEnquiryCategory === 'Water wash paint booth' ? 'WC' : 'EC';
        $msg.html('Booth height H (' + H + ' mm) must be greater than ' + chamberLbl + ' height (' + EC + ' mm).');
        $err.show();
        $('#submitbtn').prop('disabled', true);
        $('#SubmitError').show();
    } else {
        $err.hide();
        $('#SubmitError').hide();
        checkEnableSubmit();
    }
}

function calculateFilterFrame() {
    var roundedVal = parseFloat($('#txtCapacityofBlowerRoundOfmpersec').val()) || 0;
    var originalVal = parseFloat($('#txtCapacityofBlower').val()) || 0;
    var capacity = roundedVal > 0 ? roundedVal : originalVal;
    var velocity = parseFloat($('#txtVelocityofFilterFrame').val()) || 0;
    $('#txtFilterArea').val(velocity > 0 ? Math.round(capacity / velocity) : 0);
}

/* ─────────────────────────────────────────────────────────────
   BLOWER / DUCT SIZE / HP
───────────────────────────────────────────────────────────── */
function calculateCapacityPerBlower() {
    var totalCapacity = parseFloat($('#txtCapacityofBlowerinH').val()) || 0;
    var capacityRoundOf = parseFloat($('#txtCapacityofBlowerRoundOf').val()) || 0;
    var noOfBlowers = parseFloat($('#txtNoOfBlowers').val()) || 0;
    var capacity = capacityRoundOf > 0 ? capacityRoundOf : totalCapacity;

    if (noOfBlowers > 0) {
        $('#txtCapacityofeachBlower').val(Math.round(capacity / noOfBlowers));
    } else {
        $('#txtCapacityofeachBlower').val('');
    }
    calculateBlowerHp();
}

function calculateDuctWidthAndHeight() {
    var capacity = parseFloat($('#txtCapacityofeachBlower').val()) || 0;
    var valueInMM = Math.ceil(Math.sqrt(capacity / (3600 * 10)) * 1000);
    $('#txtexhaustheight').val(Math.round(valueInMM));
    adjustDuctWidthBasedOnHeight();
}

function adjustDuctWidthBasedOnHeight() {
    var height = parseFloat($('#txtexhaustheight').val()) || 0;
    var capacity = parseFloat($('#txtCapacityofeachBlower').val()) || 0;
    if (height <= 0) return;

    $('#txtExhaustWidth').val(Math.round(Math.ceil((capacity / (3600 * 10)) * 1000 * 1000) / height));
    recalcAllDuctWeights();
    renderDuctThreeD();
}

function calculateBlowerHp() {
    var capacityPerBlower = parseFloat($('#txtCapacityofeachBlower').val()) || 0;
    var waterColumn = parseFloat($('#txtWater').val()) || 0;
    // 1.2 factor added as per client sheet on 03-09-2026
    var hp = (capacityPerBlower * waterColumn * 1.2) / (3600 * 75 * 0.67);
    $('#txtHPCalculations').val(Math.round(hp));
    checkEnableSubmit();
    checkEnableFetchMotor();
}

/* ─────────────────────────────────────────────────────────────
   DOORS
───────────────────────────────────────────────────────────── */
function calculateHingedDoorWidth() {
    var W1 = parseFloat($('#txtw1').val()) || 0;
    var W2 = parseFloat($('#txtw2').val()) || 0;
    var W = parseFloat($('#txtw').val()) || 0;
    var totalHinge = W - ((W1 + W2) - 500);
    $('#txtHingedDoorWidth').val(totalHinge > 0 ? Math.round(totalHinge) : '');
    validateDoorDimensions();
}

function validateDoorDimensions() {
    var W = parseFloat($('#txtw').val()) || 0;
    var H = parseFloat($('#txtH').val()) || 0;
    var doorHeight = parseFloat($('#txtHingedDoorHeight').val()) || 0;
    var doorWidth = parseFloat($('#txtHingedDoorWidth').val()) || 0;
    $('#doorHeightError').toggle(doorHeight > H && H > 0);
    $('#doorWidthError').toggle(doorWidth > W && W > 0);
}

function validateServiceDoor() {
    if (!$('#chkServiceDoor').is(':checked')) {
        $('#serviceDoorError').hide();
        return true;
    }

    var D3 = parseFloat($('#txtD3').val()) || 0;
    var doorWidth = parseFloat($('#txtDoorWidth').val()) || 0;
    var doorHeight = parseFloat($('#txtDoorHeight').val()) || 0;
    var $errDiv = $('#serviceDoorError');
    var $errMsg = $('#serviceDoorErrorMsg');

    if (doorWidth > D3) {
        $errMsg.text('Service Door Width (' + doorWidth + ') cannot exceed D3 (' + D3 + ' mm).');
        $errDiv.show(); return false;
    }
    if (doorHeight > 2390) {
        $errMsg.text('Service Door Height cannot exceed 2390 mm.');
        $errDiv.show(); return false;
    }
    $errDiv.hide(); return true;
}

/* ─────────────────────────────────────────────────────────────
   LABELS
───────────────────────────────────────────────────────────── */
function updateFilterFrameTitle(category) {
    var isWaterWash = category === 'Water wash paint booth';
    $('#filterFrameSectionTitle').text(isWaterWash ? 'Washing Chamber Internal Design' : 'Filter Frame Design');
    $('#lblChamberHeight').text(isWaterWash ? 'WC Height (mm)' : 'EC Height (mm)');
}

/* ─────────────────────────────────────────────────────────────
   ITEM CODES
───────────────────────────────────────────────────────────── */
function generateBlowerItemCode() {
    var capacityRoundOf = parseFloat($('#txtCapacityofBlowerRoundOf').val()) || 0;
    var capacityReadonly = parseFloat($('#txtCapacityofBlowerinHReadonly').val()) || 0;
    var cmh = capacityRoundOf > 0 ? capacityRoundOf : capacityReadonly;
    var segB = String(Math.round(cmh / 1000)).padStart(3, '0');
    var pressure = parseInt($('#txtStaticPressure').val()) || 0;
    var segC = String(pressure).padStart(3, '0');
    var mountVal = $('#drpBlowerLocation').val();
    var driveVal = $('#drpDrivenType').val();
    var d1 = mountVal === 'FOOT MOUNTED' ? '1' : mountVal === 'FLANGE MOUNTED' ? '2' : '0';
    var d2 = driveVal === 'DIRECT DRIVEN' ? '1' : driveVal === 'INDIRECT DRIVEN' ? '2' : '0';
    var serial = String(Math.min(9, Math.max(0, parseInt($('#txtBlowerSerial').val()) || 0)));

    $('#txtBlowerItemCode').val('BLR-' + segB + '-' + segC + '-' + (d1 + d2 + serial));
    $('#blowerCodeBreakdown').html(
        codeBadge('BLR', 'Type') +
        codeBadge(segB, 'Cap (' + Math.round(cmh) + ' CMH)') +
        codeBadge(segC, 'Pressure (mm)') +
        codeBadge(d1, mountVal === 'FOOT MOUNTED' ? 'Foot mounted' : mountVal === 'FLANGE MOUNTED' ? 'Flange mounted' : '—') +
        codeBadge(d2, driveVal === 'DIRECT DRIVEN' ? 'Direct driven' : driveVal === 'INDIRECT DRIVEN' ? 'Indirect driven' : '—') +
        codeBadge(serial, 'Serial')
    );
}

function generateMotorItemCode() {
    var hp = parseFloat(selectedMotorHP) || 0;
    var segB = String(Math.round(hp * 10)).padStart(3, '0');
    var phase = $('#drpMotorPhase').val() || '3';
    var poles = $('#drpMotorPoles').val() || '4';
    var proofRaw = $('#drpMotorProof').val() || 'NFLP';
    var proof = proofRaw === 'FLP' ? 'F' : 'N';
    var mount = $('#drpMotorMount').val() || '1';
    var ie = $('#drpMotorIE').val() || '2';
    var serial = String(Math.min(9, Math.max(0, parseInt($('#txtMotorSerial').val()) || 0)));
    var ieMap = { '1': 'IE1', '2': 'IE2', '3': 'IE3', '4': 'EFF1', '5': 'EFF2', '6': 'EFF3' };

    $('#txtMotorItemCode').val('MTR-' + segB + '-' + (phase + poles + proof) + '-' + (mount + ie + serial));
    $('#motorCodeBreakdown').html(
        codeBadge('MTR', 'Type') +
        codeBadge(segB, 'HP (' + hp + ')') +
        codeBadge(phase, phase === '1' ? 'Single phase' : 'Three phase') +
        codeBadge(poles, poles + ' Pole') +
        codeBadge(proof, proofRaw === 'FLP' ? 'FLP' : 'NFLP') +
        codeBadge(mount, mount === '1' ? 'Foot mounted' : 'Flange mounted') +
        codeBadge(ie, ieMap[ie] || ie) +
        codeBadge(serial, 'Serial')
    );
}

function codeBadge(val, lbl) {
    return '<div class="pb-code-badge"><span class="pb-code-badge-val">' + val + '</span><span class="pb-code-badge-lbl">' + lbl + '</span></div>';
}

/* ─────────────────────────────────────────────────────────────
   SUBMIT / FETCH ENABLE
───────────────────────────────────────────────────────────── */
function checkEnableSubmit() {
    var panelOk = !$('#panelHeightAboveFilterError').is(':visible');
    var motorOk = $('#divMotorItemCode').is(':visible');
    $('#submitbtn').prop('disabled', !(panelOk && motorOk));
}

function checkEnableFetchMotor() {
    var hp = parseFloat($('#txtHPCalculations').val()) || 0;
    $('#btnFetchRecords').prop('disabled', hp <= 0);
}

/* ─────────────────────────────────────────────────────────────
   MOTOR — global so restoreMotorForEdit can use it
───────────────────────────────────────────────────────────── */
function bindMotorRadio() {
    $(document).off('change', '.motor-radio').on('change', '.motor-radio', function () {
        $('#motorCatalogIDInput').val($(this).data('id'));
        selectedMotorHP = parseFloat($(this).data('hp')) || 0;
        $('#divMotorItemCode').show();
        generateMotorItemCode();
        checkEnableSubmit();
    });
}

// Called AFTER water column is restored so HP is correct
function restoreMotorForEdit(savedMotorId) {
    if (!savedMotorId || savedMotorId <= 0) return;
    savedMotorId = parseInt(savedMotorId);

    var hp = parseFloat($('#txtHPCalculations').val()) || 0;

    $.ajax({
        url: '/PaintBoothDesign/GetMotorDetails',
        type: 'GET',
        data: { blowerHpCalculation: hp },
        success: function (data) {
            if (!data || data.length === 0) {
                PB.toast('No motors found for saved HP value.', 'warning');
                return;
            }

            var $body = $('#motorFlangeBody').empty();
            var foundMotor = false;

            $.each(data, function (i, m) {
                var isSelected = m.motorCatalogID === savedMotorId;
                if (isSelected) {
                    selectedMotorHP = parseFloat(m.ratedOutputHP) || 0;
                    foundMotor = true;
                }

                $body.append(
                    '<tr' + (isSelected ? ' class="pb-selected-row"' : '') + '>' +
                    '<td>' + m.motorCatalogID + '</td>' +
                    '<td>' + m.ratedOutputHP + '</td>' +
                    '<td>' + m.typeReference + '</td>' +
                    '<td>' + m.ratedSpeed + '</td>' +
                    '<td>' + m.ratedCurrent + '</td>' +
                    '<td>' + m.efficiencyFullLoad + '</td>' +
                    '<td style="text-align:center;">' +
                    '<input type="radio" name="motorSelect" class="motor-radio" ' +
                    'data-id="' + m.motorCatalogID + '" ' +
                    'data-hp="' + m.ratedOutputHP + '"' +
                    (isSelected ? ' checked' : '') + ' />' +
                    '</td>' +
                    '</tr>'
                );
            });

            $('#motorTableSection').show();

            if (foundMotor) {
                $('#motorCatalogIDInput').val(savedMotorId);
                $('#divMotorItemCode').show();
                generateMotorItemCode();
                checkEnableSubmit();
                PB.toast('Motor restored — ID ' + savedMotorId, 'info');
            } else {
                PB.toast('Previously selected motor not in range. Please re-select.', 'warning');
            }

            bindMotorRadio();
        },
        error: function () {
            PB.toast('Failed to load motor details.', 'danger');
        }
    });
}

/* ─────────────────────────────────────────────────────────────
   DOCUMENT READY
───────────────────────────────────────────────────────────── */
$(function () {

    /* ── INITIAL STATE ───────────────────────────────────────── */
    $('#divAllSections').hide();
    $('#motorTableSection').hide();
    $('#pressureDropSection').hide();
    $('#divMotorItemCode').hide();
    $('#serviceDoorFields').hide();
    $('#watchGlassFields').hide();
    $('#dicductlength').hide();
    $('#dicductorient').hide();
    $('#dicbenddir').show();

    renderDuctThreeD();
    setTimeout(function () { window.dispatchEvent(new Event('resize')); }, 100);

    // Edit mode = record already exists (PaintBoothID > 0)
    var isEditMode = (parseInt($('#PaintBoothID').val()) || 0) > 0;

    // Only fill a field if it has no value
    function setDefault(selector, value) {
        var $el = $(selector);
        var cur = $.trim($el.val() || '');
        if (!cur || cur === '0') $el.val(value);
    }

    if (!isEditMode) {
        setDefault('#txtD1', '1200');
        setDefault('#txtD2', '700');
        setDefault('#txtD3', '800');
        setDefault('#txtw1', '500');
        setDefault('#txtw2', '500');
        setDefault('#txtH1', '700');
        setDefault('#txtH2', '1000');
        setDefault('#txtExhaustThickness', '2');
        setDefault('#BetweenCenterDistance', '2000');
        setDefault('#txtVelocityofBlower', '0.5');
        setDefault('#txtVelocityofFilterFrame', '0.5');
        setDefault('#txtDoorHeight', '1180');
        setDefault('#txtDoorWidth', '580');
        setDefault('#drpServiceDoorLocation', 'RightSide');
    }

    // Shared: show/hide sections based on sub type
    function applySubTypeVisibility(subType) {
        $('#divComponentEntry, #divViewingGlass').toggle(subType != "1");
        $('#PanelsAboveFilterFrame').toggle(!(subType == "4" || subType == "5"));
        $('#DivBetweenCenterDistance').toggle(subType == "4");
    }

    function setBlowerDefaults(subType) {
        var noOfBlowers = (subType == "8" || subType == "9") ? 2 : 1;
        $('#txtNoOfBlowers').val(noOfBlowers);
        calculateCapacityPerBlower();
    }

    /* ── 1. ENQUIRY CODE AUTOCOMPLETE ───────────────────────── */
    $('#txtEnquiryCode').on('input', function () {
        var term = $.trim($(this).val());
        $('#searchbtn').prop('disabled', term.length === 0);
        if (term.length < 1) { $('#resultsContainer').hide().empty(); return; }
        $.ajax({
            url: '/PaintBoothDesign/SearchEnquiryCode',
            type: 'POST',
            data: { enquiryCode: term },
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (res) {
                var $dd = $('#resultsContainer').empty();
                if (res.success && res.results.length > 0) {
                    $.each(res.results, function (i, code) { $('<div class="pb-dropdown-item">').text(code).appendTo($dd); });
                    $dd.show();
                } else {
                    $dd.append('<div class="pb-dropdown-item" style="color:var(--text-muted);">No results</div>').show();
                }
            }
        });
    });

    $(document).on('click', '.pb-dropdown-item', function () {
        var code = $(this).text();
        if (code === 'No results') return;
        $('#txtEnquiryCode').val(code);
        $('#resultsContainer').hide().empty();
        $('#searchbtn').prop('disabled', false);
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#txtEnquiryCode, #resultsContainer').length) $('#resultsContainer').hide();
    });

    /* ── 2. SEARCH BUTTON ───────────────────────────────────── */
    $('#searchbtn').on('click', function () {
        var code = $.trim($('#txtEnquiryCode').val());
        if (!code) return;
        $.ajax({
            url: '/PaintBoothDesign/Search',
            type: 'POST',
            data: { enquiryCode: code },
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (data) {
                if (data.error) { PB.toast(data.error, 'danger'); return; }

                $('#txtlenght').val(data.length || '');
                $('#txtwidth').val(data.width || '');
                $('#txtheight').val(data.height || '');
                $('#txtExtractionChamberHeight').val(data.extractionChamberHeight || '');
                $('#drpMotorProof').val(data.motorTypes || '');
                standardPanelSize = parseFloat(data.panelWidth) || 1140;
                standardPanelHeight = parseFloat(data.panelHeight) || 2390;

                // paintbooth type, e.g. dry back paint booth or water wash paint booth
                currentEnquiryCategory = data.category || '';
                updateFilterFrameTitle(data.category);

                $('#divAllSections').show();
                applySubTypeVisibility(data.subTypeOfDraftType);
                $('#showSettingDetails').prop('disabled', false);
                setTimeout(function () { window.dispatchEvent(new Event('resize')); }, 300);

                var st = data.subTypeOfDraftType;
                calculateD(st); calculateW(st); calculateH(st);
                calculateCrossSectionalArea(st); calculateCapacity(); calculateFilterFrame();
                calculatePanels(standardPanelSize); calculatePanelsforW(standardPanelHeight);
                calculatePanelsforH(standardPanelHeight); updateTxtW3();
                setBlowerDefaults(st);              // sets No. of blowers + capacity per blower + HP
                calculateDuctWidthAndHeight();      // needs capacity per blower
                calculatePanelHeightAboveFilterFrame(); calculateHingedDoorWidth(); checkEnableFetchMotor();

                PB.toast('Enquiry data loaded — ' + code, 'success');
            },
            error: function () { PB.toast('Error loading enquiry. Please try again.', 'danger'); }
        });
    });

    /* ── 3. SETTING DETAILS MODAL ───────────────────────────── */
    $('#showSettingDetails').on('click', function () {
        var code = $('#txtEnquiryCode').val();
        function row(label, val) { return '<tr><th>' + label + '</th><td>' + (val || '—') + '</td></tr>'; }
        $.ajax({
            url: '/PaintBoothDesign/GetSettingDetailsByCode',
            type: 'GET',
            data: { enquiryCode: code },
            success: function (data) {
                var $body = $('#settingModalContent').empty();
                if (!data.settings || data.settings.length === 0) {
                    $body.html('<p style="color:var(--text-muted);">No settings found.</p>');
                } else {
                    $.each(data.settings, function (i, s) {
                        var typeClass = s.settingType === 'Paintbooth' ? 'pb-setting-type-pb' : 'pb-setting-type-ec';
                        $body.append('<div class="mb-3"><span class="pb-setting-type-badge ' + typeClass + '">' + s.settingType + '</span><table class="pb-setting-table">' +
                            row('Panel Width', s.panelWidth) + row('Panel Height', s.panelHeight) +
                            row('Sheet Thickness', s.sheetThickness) + row('Section', s.section) +
                            row('Bend / Structural', s.bendStructural) + row('Standard Bend 1', s.standardBend1) +
                            row('Standard Bend 2', s.standardBend2) + row('Pitch Distance', s.pitchDistance) +
                            row('Slot Dimensions', s.slotDimensions) + row('Materials', s.materials) +
                            row('Light Type', s.lightTypes) + row('Light Sub Type', s.lightSubTypes) +
                            row('Lux Level', s.luxLevel) + row('Lumens', s.lumens) +
                            '</table></div>');
                    });
                }
                $('#settingModalOverlay').show();
            }
        });
    });

    $('#closeSettingModal, #closeSettingModal2').on('click', function () { $('#settingModalOverlay').hide(); });
    $('#settingModalOverlay').on('click', function (e) { if ($(e.target).is('#settingModalOverlay')) $(this).hide(); });

    /* ── 4. D1, D2 INPUT CHANGES ────────────────────────────── */
    $('#txtD1, #txtD2, #txtlenght').on('input', function () { calculateD(); calculateCrossSectionalArea(); calculateCapacity(); });
    $('#txtD').on('input', function () { calculatePanels(standardPanelSize); });

    /* ── 5. W1, W2 INPUT CHANGES ────────────────────────────── */
    $('#txtw1, #txtw2, #txtwidth').on('input', function () { calculateW(); });

    /* ── 6. H1, H2 INPUT CHANGES ────────────────────────────── */
    $('#txtH1, #txtH2, #txtheight').on('input', function () { isManualHChange = false; calculateH(); });
    $('#txtH').on('input', function () { isManualHChange = true; calculatePanelsforH(standardPanelHeight); calculatePanelHeightAboveFilterFrame(); });

    /* ── 7. SERVICE DOOR ────────────────────────────────────── */
    $('#chkServiceDoor').on('change', function () { $('#serviceDoorFields').toggle($(this).is(':checked')); });
    if ($('#chkServiceDoor').is(':checked')) { $('#serviceDoorFields').show(); }   // edit mode
    $('#txtDoorWidth, #txtDoorHeight').on('input', validateServiceDoor);

    /* ── 8. VIEWING GLASS ───────────────────────────────────── */
    $('#chkWatchDoor').on('change', function () {
        if ($(this).is(':checked')) { $('#watchGlassFields').slideDown(); }
        else { $('#watchGlassFields').slideUp(); $('#txtWatchglassHeight').val(''); $('#WatchglassWidth').val(''); }
    });
    if ($('#chkWatchDoor').is(':checked')) { $('#watchGlassFields').show(); }       // edit mode

    /* ── 9. DOOR DIMENSION VALIDATION ──────────────────────── */
    $('#txtHingedDoorHeight, #txtHingedDoorWidth').on('input', validateDoorDimensions);

    /* ── 10. VELOCITY INPUTS ────────────────────────────────── */
    $('#txtVelocityofBlower').on('input', calculateCapacity);
    $('#txtVelocityofFilterFrame').on('input', calculateFilterFrame);

    /* ── 11. ROUND UP CAPACITY ──────────────────────────────── */
    $('#txtCapacityofBlowerRoundOf').on('input', function () {
        var m3PerHr = parseFloat($(this).val()) || 0;
        $('#txtCapacityofBlowerRoundOfmpersec').val((m3PerHr / 3600).toFixed(3));
        calculateFilterFrame();
        calculateCapacityPerBlower();       // also recalculates HP
        generateBlowerItemCode();
        calculateDuctWidthAndHeight();
    });
    $('#txtexhaustheight').on('change', adjustDuctWidthBasedOnHeight);

    /* ── 11a. NO. OF BLOWERS ────────────────────────────────── */
    $('#txtNoOfBlowers').on('input', function () {
        var val = parseInt($(this).val()) || 0;
        if (val <= 0 || (val !== 1 && val % 2 !== 0)) {
            PB.toast('No. of Blowers must be 1 or an even number.', 'error');
            $(this).val('');
            $('#txtCapacityofeachBlower').val('');
            $('#txtHPCalculations').val('');
            return;
        }
        calculateCapacityPerBlower();
        calculateDuctWidthAndHeight();
    });

    /* ── 12. WATER COLUMN MANUAL INPUT ─────────────────────── */
    $('#txtWater').on('input', function () {
        var wc = parseFloat($(this).val()) || 0;
        $('#txtStaticPressure').val(wc > 0 ? Math.ceil(wc) : 0);
        calculateBlowerHp();
        generateBlowerItemCode();
    });

    /* ── 13. DUCTING DROPDOWN ───────────────────────────────── */
    $('#drpBendDucting').on('change', function () {
        var v = $(this).val();
        $('#dicductlength').toggle(v === 'Duct');
        $('#dicductorient').toggle(v === 'Duct');
        $('#dicbenddir').toggle(v === 'Bend');
        if (v !== 'Duct') $('#txtDuctLength').val('');
    });

    /* ── 14. ADD / DELETE / EDIT DUCTING ROW ────────────────── */
    $('#btnAdd').on('click', function () {
        var bendType = $('#drpBendDucting').val();
        var ductLength = $('#txtDuctLength').val();
        var bendDir = $('#drpBendDirection').val();
        var ductOrient = $('#drpDuctOrientation').val();

        if (!bendType) { PB.toast('Please select a Ducting Component.', 'warning'); return; }
        if (bendType === 'Duct' && !ductLength) { PB.toast('Please enter Duct Length.', 'warning'); return; }

        $('#emptyDuctRow').remove();

        var mmLen = bendType === 'Duct' ? (parseInt(ductLength) || 0) : 0;
        var netWt = ductRowNetWeight(bendType, mmLen);
        var grossWt = ductRowGrossWeight(bendType, mmLen);

        $('#componentsTable tbody').append(
            buildDuctRowHtml(ductRowIndex++, bendType, mmLen, bendDir, ductOrient, netWt, grossWt)
        );

        recalcAllDuctWeights();             // totals + hidden JSON
        renderDuctThreeD();
        $('#drpBendDucting').val('Bend').trigger('change');
        $('#txtDuctLength').val('');
    });

    $(document).on('click', '.duct-del', function () {
        $(this).closest('tr').remove();
        updateDuctRowNumbers();
        recalcAllDuctWeights();
        renderDuctThreeD();
        if ($('#componentsTable tbody tr[data-duct]').length === 0) {
            $('#componentsTable tbody').html('<tr id="emptyDuctRow"><td colspan="6" style="text-align:center;color:var(--text-muted);padding:20px;">No ducting components added</td></tr>');
        }
    });

    $(document).on('click', '.duct-edit', function () {
        var $row = $(this).closest('tr');
        var bendType = $row.find('.duct-type').text().trim();
        var bendDir = $row.data('dir');

        $('#drpBendDucting').val(bendType).trigger('change');   // handles show/hide

        if (bendType === 'Duct') {
            $('#txtDuctLength').val($row.data('len'));
            $('#drpDuctOrientation').val($row.data('orient') || 'Vertical');
        } else if (bendDir) {
            $('#drpBendDirection').val(bendDir);
        }

        $row.remove();
        updateDuctRowNumbers();
        recalcAllDuctWeights();
        renderDuctThreeD();
    });

    /* ── DUCT WEIGHT RECALC ON DIMENSION CHANGE ─────────────── */
    $('#txtExhaustWidth, #txtexhaustheight, #txtExhaustThickness').on('input', function () {
        recalcAllDuctWeights();
        renderDuctThreeD();
    });

    /* ── DUCT THICKNESS VALIDATION ──────────────────────────── */
    $('#txtExhaustThickness').on('input blur', function () {
        var val = parseFloat($(this).val());
        if (isNaN(val) || val <= 0) {
            $('#ductThicknessError').show();
            $(this).addClass('pb-input-error');
        } else {
            $('#ductThicknessError').hide();
            $(this).removeClass('pb-input-error');
        }
    });

    /* ── 15. WATER COLUMN BUTTON ────────────────────────────── */
    $('#btnWaterColumn').on('click', function (e, restoreCallback) {
        var bendCount = 0;
        $('#componentsTable tbody tr[data-duct]').each(function () {
            if ($(this).find('.duct-type').text().indexOf('Bend') > -1) bendCount++;
        });

        $.ajax({
            url: '/PaintBoothDesign/WaterColumnCalculations',
            type: 'GET',
            data: { category: currentEnquiryCategory },
            success: function (data) {
                var $body = $('#pressureDropBody').empty();
                if (!data.pressureDropDetails || data.pressureDropDetails.length === 0) {
                    $body.append('<tr><td colspan="6" style="text-align:center;color:var(--text-muted);">No data</td></tr>');
                    $('#pressureDropSection').show();
                    return;
                }

                // Display order: listed items first, everything else after (original order)
                var pdOrder = [
                    'washing chamber pressure drop',
                    'ec transition piece',
                    'transition piece',
                    'damper'
                ];

                function pdRank(desc) {
                    var idx = pdOrder.indexOf($.trim(desc || '').toLowerCase());
                    return idx === -1 ? pdOrder.length : idx;
                }

                data.pressureDropDetails = data.pressureDropDetails
                    .map(function (item, i) { return { item: item, i: i }; })
                    .sort(function (a, b) {
                        return (pdRank(a.item.description) - pdRank(b.item.description)) || (a.i - b.i);
                    })
                    .map(function (x) { return x.item; });

                $.each(data.pressureDropDetails, function (i, item) {
                    var d = $.trim(item.description || '').toLowerCase();
                    var qty = d === '90 deg. bend' ? bendCount
                        : d === 'washing chamber pressure drop' ? 1
                            : d === '135 deg. bend' ? 1
                                : '';
                    var pd = parseFloat(item.pressureDrop_mm) || 0;
                    var editable = pd === 0;
                    var desc = item.description;
                    if (currentEnquiryCategory === 'Water wash paint booth' && d === 'ec transition piece') {
                        desc = 'WC Transition piece';
                    }

                    // Editable input when DB value is 0, otherwise plain text
                    var pdCell = editable
                        ? '<input type="number" class="pb-input pd-pressure pd-pressure-edit" style="width:80px;" min="0" step="0.1" placeholder="Enter" />'
                        : '<span class="pd-pressure">' + pd + '</span>';

                    $body.append(
                        '<tr' + (editable ? ' data-pd-editable="1"' : '') + '>' +
                        '<td><input type="checkbox" class="pd-chk" data-id="' + item.itemNumber + '" checked /></td>' +
                        '<td style="text-align:center;">' + (i + 1) + '</td>' +
                        '<td>' + desc + '</td>' +
                        '<td style="text-align:center;">' + pdCell + '</td>' +
                        '<td><input type="number" class="pb-input pd-qty" style="width:70px;" value="' + qty + '" /></td>' +
                        '<td><input type="number" class="pb-input pd-total" style="width:70px;" readonly /></td>' +
                        '</tr>'
                    );
                });

                bindPressureDropEvents();
                $('#pressureDropSection').show();

                if (typeof restoreCallback === 'function') {
                    restoreCallback();                               // edit mode: restore saved quantities
                } else {
                    $('#pressureDropBody .pd-qty').trigger('input'); // create mode
                }
            },
            error: function () { PB.toast('Failed to load water column data.', 'danger'); }
        });
    });

    function getRowPressure($row) {
        var $p = $row.find('.pd-pressure');
        if (!$p.length) return 0;
        return parseFloat($p.is('input') ? $p.val() : $p.text()) || 0;
    }

    function recalcPdRow($row) {
        var qty = parseFloat($row.find('.pd-qty').val()) || 0;
        var pressure = getRowPressure($row);
        var on = $row.find('.pd-chk').prop('checked');
        $row.find('.pd-total').val(on && qty && pressure ? (qty * pressure).toFixed(2) : '');
    }

    function bindPressureDropEvents() {
        $(document).off('change', '.pd-chk').on('change', '.pd-chk', function () {
            var $row = $(this).closest('tr');
            $row.find('.pd-qty, .pd-pressure-edit').prop('disabled', !$(this).prop('checked'));
            recalcPdRow($row);
            updateGrandTotal();
        });

        // Quantity OR editable pressure drop → recalc row + grand total
        $(document).off('input', '.pd-qty, .pd-pressure-edit')
            .on('input', '.pd-qty, .pd-pressure-edit', function () {
                recalcPdRow($(this).closest('tr'));
                updateGrandTotal();
            });

        $('#additionFactor').off('input').on('input', updateGrandTotal);
    }

    function updateGrandTotal() {
        var total = 0;
        var rows = [];

        $('.pd-total').each(function () {
            var $row = $(this).closest('tr');
            var isChecked = $row.find('.pd-chk').prop('checked');
            if (isChecked) total += parseFloat($(this).val()) || 0;
            rows.push({
                itemNumber: parseInt($row.find('.pd-chk').data('id')),
                quantity: parseFloat($row.find('.pd-qty').val()) || 0,
                isSelected: isChecked
            });
        });

        total += parseFloat($('#additionFactor').val()) || 0;

        $('#grandTotal').text(total.toFixed(2));
        $('#txtWater').val(total.toFixed(2));
        $('#hiddenWaterColumnRows').val(JSON.stringify(rows));
        $('#txtStaticPressure').val(total > 0 ? Math.ceil(total) : 0);
        calculateBlowerHp();
        generateBlowerItemCode();
    }

    /* ── 16. MOTOR DETAILS BUTTON ───────────────────────────── */
    $('#btnFetchRecords').on('click', function () {
        var hp = $('#txtHPCalculations').val();
        $.ajax({
            url: '/PaintBoothDesign/GetMotorDetails',
            type: 'GET',
            data: { blowerHpCalculation: hp },
            success: function (data) {
                var $body = $('#motorFlangeBody').empty();
                if (!data || data.length === 0) {
                    $body.append('<tr><td colspan="7" style="text-align:center;color:var(--text-muted);">No motors found</td></tr>');
                } else {
                    $.each(data, function (i, m) {
                        $body.append(
                            '<tr><td>' + m.motorCatalogID + '</td><td>' + m.ratedOutputHP + '</td>' +
                            '<td>' + m.typeReference + '</td><td>' + m.ratedSpeed + '</td>' +
                            '<td>' + m.ratedCurrent + '</td><td>' + m.efficiencyFullLoad + '</td>' +
                            '<td style="text-align:center;"><input type="radio" name="motorSelect" class="motor-radio" data-id="' + m.motorCatalogID + '" data-hp="' + m.ratedOutputHP + '" /></td></tr>'
                        );
                    });
                }
                $('#motorTableSection').show();
                bindMotorRadio();
            },
            error: function () { PB.toast('Failed to load motor details.', 'danger'); }
        });
    });

    /* ── 17. ITEM CODE TRIGGERS ─────────────────────────────── */
    $('#drpBlowerLocation, #drpDrivenType').on('change', generateBlowerItemCode);
    $('#txtBlowerSerial, #txtStaticPressure').on('input', generateBlowerItemCode);
    $('#drpMotorPhase, #drpMotorPoles, #drpMotorProof, #drpMotorMount, #drpMotorIE').on('change', generateMotorItemCode);
    $('#txtMotorSerial, #drpMotorProof').on('input', generateMotorItemCode);

    /* ── 18. FORM SUBMIT ────────────────────────────────────── */
    $('#pbForm').on('submit', function () {
        if (!validateServiceDoor()) return false;

        if ($('#componentsTable tbody tr[data-duct]').length === 0) {
            PB.toast('At least one ducting component (Bend or Duct) is required.', 'danger');
            $('html, body').animate({ scrollTop: $('#componentsTable').offset().top - 120 }, 300);
            return false;
        }

        var thickness = parseFloat($('#txtExhaustThickness').val());
        if (isNaN(thickness) || thickness <= 0) {
            PB.toast('Duct Thickness must be greater than 0.', 'danger');
            $('#txtExhaustThickness').addClass('pb-input-error').focus();
            return false;
        }

        recalcAllDuctWeights();             // make sure saved JSON matches the current dimensions
        $('#pbLoader').show();
        $('#submitbtn').addClass('loading').prop('disabled', true);
    });

    /* ── 19. EDIT MODE ──────────────────────────────────────── */
    var prefilledCode = $.trim($('#txtEnquiryCode').val());

    if (prefilledCode) {
        $('#txtEnquiryCode').prop('readonly', true);

        $.ajax({
            url: '/PaintBoothDesign/Search',
            type: 'POST',
            data: { enquiryCode: prefilledCode },
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (data) {
                if (data.error) { PB.toast(data.error, 'danger'); return; }

                $('#txtlenght').val(data.length || '');
                $('#txtwidth').val(data.width || '');
                $('#txtheight').val(data.height || '');
                $('#txtExtractionChamberHeight').val(data.extractionChamberHeight || '');
                $('#drpMotorProof').val(data.motorTypes || '');

                standardPanelSize = parseFloat(data.panelWidth) || 1140;
                standardPanelHeight = parseFloat(data.panelHeight) || 2390;

                currentEnquiryCategory = data.category || '';
                updateFilterFrameTitle(data.category);

                $('#divAllSections').show();
                applySubTypeVisibility(data.subTypeOfDraftType);
                $('#showSettingDetails').prop('disabled', false);
                setTimeout(function () { window.dispatchEvent(new Event('resize')); }, 300);

                var st = data.subTypeOfDraftType;
                calculateD(st); calculateW(st); calculateH(st);
                calculateCrossSectionalArea(st); calculateCapacity(); calculateFilterFrame();
                calculatePanels(standardPanelSize); calculatePanelsforW(standardPanelHeight);
                calculatePanelsforH(standardPanelHeight); updateTxtW3();
                calculateCapacityPerBlower();       // first — uses saved No. of blowers
                calculateDuctWidthAndHeight();      // then duct size
                calculatePanelHeightAboveFilterFrame(); calculateHingedDoorWidth();

                PB.toast('Design loaded for editing — ' + prefilledCode, 'info');

                // ── Restore saved ducting rows ──
                var savedDucts = [];
                try { savedDucts = JSON.parse($('#hiddenDuctingList').val() || '[]'); }
                catch (e) { savedDucts = []; }

                if (savedDucts.length) {
                    $('#emptyDuctRow').remove();
                    savedDucts.forEach(function (d) {
                        var isDuct = d.BendType === 'Duct';
                        $('#componentsTable tbody').append(
                            buildDuctRowHtml(ductRowIndex++, d.BendType, d.DuctLength || 0,
                                isDuct ? null : d.BendDirection,     // bend direction
                                isDuct ? d.BendDirection : null,     // duct orientation
                                d.DuctNetWeight, d.DuctGrossWeight)
                        );
                    });
                    recalcAllDuctWeights();         // replace stale DB weights with current ones
                    renderDuctThreeD();
                }

                // ── Restore water column + motor ──
                var savedWaterColumn = parseFloat($('#txtWater').val()) || 0;

                if (savedWaterColumn > 0) {
                    $('#txtStaticPressure').val(Math.ceil(savedWaterColumn));
                    $('#txtWater').val(savedWaterColumn.toFixed(2));
                    calculateBlowerHp();

                    $('#btnWaterColumn').trigger('click', [function () {
                        var savedRows = [];
                        try { savedRows = JSON.parse($('#hiddenWaterColumnRows').val() || '[]'); }
                        catch (e) { console.warn('Could not parse water column rows:', e); }

                        savedRows.forEach(function (r) {
                            var $chk = $('.pd-chk[data-id="' + r.itemNumber + '"]');
                            if ($chk.length) {
                                var $row = $chk.closest('tr');
                                $chk.prop('checked', r.isSelected);
                                $row.find('.pd-qty').val(r.quantity).trigger('input');
                            }
                        });

                        $('#additionFactor').trigger('input');
                        PB.toast('Water column restored — ' + savedWaterColumn.toFixed(2) + ' mm', 'info');
                        calculateBlowerHp();

                        // Read motor ID after DOM and calculations are settled
                        var savedMotorId = parseInt($('#motorCatalogIDInput').val()) || 0;
                        if (savedMotorId > 0) restoreMotorForEdit(savedMotorId);
                    }]);

                } else {
                    calculateBlowerHp();
                    checkEnableFetchMotor();

                    var savedMotorId = parseInt($('#motorCatalogIDInput').val()) || 0;
                    if (savedMotorId > 0) restoreMotorForEdit(savedMotorId);
                }
            },
            error: function () { PB.toast('Error loading enquiry data.', 'danger'); }
        });
    }
});
