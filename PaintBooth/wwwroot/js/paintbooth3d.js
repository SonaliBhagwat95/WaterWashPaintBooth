/* ============================================================
   Bullows PaintBooth Pro — paintbooth3d.js  v6
   Three.js live 3D preview — Open-Type Dry PaintBooth

   TRANSITION PIECE — matches GA3D Image 1 exactly:
   • Large rectangular base = full EC top footprint
   • Smoothly tapers to a circle (blower inlet)
   • Circle is OFFSET towards the front (not centred)
   • Entire top skin + transition = one continuous yellow surface
   • Built with custom BufferGeometry (triangulated faces)

   Coordinate axes (Y-up, right-hand):
     X = depth  (0 = booth front, D = back wall)
     Y = height (0 = floor)
     Z = width  (0 = right side, W = left side)
   ============================================================ */
(function () {
    'use strict';

    /* ── Palette ────────────────────────────────────────────── */
    var C = {
        wall    : 0x2e5fa3,
        wallAlt : 0x2a5595,
        ec      : 0x1e8060,
        ecAlt   : 0x197050,
        struct  : 0x6a7d9a,
        filter  : 0xc89a2a,
        blower  : 0x3d4f6a,
        duct    : 0x4a5870,
        comp    : 0xe85d10,
        door    : 0x2a3a55,
        seam    : 0x1a2a40,
        trans   : 0xccd630,   /* yellow-green — matches GA3D */
        floor   : 0x080e18,
        sky     : 0x0d1117
    };

    var scene, cam, ren, g;
    var wire  = false;
    var ang   = { th: -0.50, ph: 1.05, dist: 22000 };
    var drag  = { on: false, x: 0, y: 0 };
    var pivot = { x: 0, y: 0, z: 0 };

    /* ════════════════════════════════════════════════════════
       BOOT — wait until canvas is visible
    ════════════════════════════════════════════════════════ */
    (function boot() {
        var cv = document.getElementById('pb3dCanvas');
        if (cv && typeof THREE !== 'undefined' && cv.clientWidth > 0) {
            setup();
        } else {
            setTimeout(boot, 120);
        }
    }());

    /* ════════════════════════════════════════════════════════
       SETUP
    ════════════════════════════════════════════════════════ */
    function setup() {
        var cv = document.getElementById('pb3dCanvas');
        if (!cv) return;

        scene = new THREE.Scene();
        scene.background = new THREE.Color(C.sky);
        scene.fog = new THREE.FogExp2(C.sky, 0.000013);

        cam = new THREE.PerspectiveCamera(40, cv.clientWidth / 420, 10, 600000);

        ren = new THREE.WebGLRenderer({ canvas: cv, antialias: true });
        ren.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        ren.setSize(cv.clientWidth, 420);

        /* 3-point lighting */
        scene.add(new THREE.AmbientLight(0xd0d8f0, 0.60));
        var key = new THREE.DirectionalLight(0xffffff, 0.85);
        key.position.set(8000, 14000, 6000); scene.add(key);
        var fill = new THREE.DirectionalLight(0x90b0ff, 0.32);
        fill.position.set(-7000, 2000, -4000); scene.add(fill);
        var rim = new THREE.DirectionalLight(0xfff0d0, 0.18);
        rim.position.set(0, -2000, 9000); scene.add(rim);

        /* Ground */
        var gnd = new THREE.Mesh(
            new THREE.PlaneGeometry(400000, 400000),
            new THREE.MeshStandardMaterial({ color: C.floor, roughness: 1 })
        );
        gnd.rotation.x = -Math.PI / 2;
        scene.add(gnd);

        /* Mouse orbit */
        cv.addEventListener('mousedown', function (e) {
            drag.on = true; drag.x = e.clientX; drag.y = e.clientY;
            cv.style.cursor = 'grabbing';
        });
        window.addEventListener('mouseup', function () {
            drag.on = false; cv.style.cursor = 'grab';
        });
        window.addEventListener('mousemove', function (e) {
            if (!drag.on) return;
            ang.th -= (e.clientX - drag.x) * 0.005;
            ang.ph  = clamp(ang.ph - (e.clientY - drag.y) * 0.005, 0.12, 1.55);
            drag.x = e.clientX; drag.y = e.clientY;
            moveCam();
        });
        cv.addEventListener('wheel', function (e) {
            e.preventDefault();
            ang.dist = clamp(ang.dist + e.deltaY * 14, 800, 220000);
            moveCam();
        }, { passive: false });

        /* Touch */
        var touch = { x: 0, y: 0 };
        cv.addEventListener('touchstart', function (e) {
            if (e.touches.length === 1) { touch.x = e.touches[0].clientX; touch.y = e.touches[0].clientY; }
        }, { passive: true });
        cv.addEventListener('touchmove', function (e) {
            if (e.touches.length !== 1) return;
            ang.th -= (e.touches[0].clientX - touch.x) * 0.004;
            ang.ph  = clamp(ang.ph - (e.touches[0].clientY - touch.y) * 0.004, 0.12, 1.55);
            touch.x = e.touches[0].clientX; touch.y = e.touches[0].clientY;
            moveCam();
        }, { passive: true });

        addBtn('btn3dWire', function () {
            wire = !wire; this.classList.toggle('active', wire); redraw();
        });
        addBtn('btn3dReset', function () {
            ang.th = -0.50; ang.ph = 1.05; moveCam();
        });

        /* Watch all dimension inputs */
        [
            'txtlenght','txtwidth','txtheight',
            'txtD1','txtD2','txtD3',
            'txtw1','txtw2','txtH1','txtH2',
            'txtD','txtw','txtH',
            'txtExtractionChamberHeight',
            'txtHingedDoorHeight','txtHingedDoorWidth',
            'chkServiceDoor','chkWatchDoor',
            'txtWatchGlassBottomDistance',
            'txtWatchglassHeight','WatchglassWidth',
            'txtstandardPanels','txtremaingPanels',
            'txtstandardPanelsByW','txtremaingPanelsByW',
            'txtstandardPanelsByH','txtremaingPanelsByH'
        ].forEach(function (id) {
            var el = document.getElementById(id);
            if (!el) return;
            el.addEventListener('change', redraw);
            el.addEventListener('input',  redraw);
        });

        window.addEventListener('resize', function () {
            var w = cv.clientWidth;
            if (w === 0) return;
            ren.setSize(w, 420);
            cam.aspect = w / 420;
            cam.updateProjectionMatrix();
            redraw();
        });

        redraw();
        (function loop() { requestAnimationFrame(loop); ren.render(scene, cam); }());
    }

    /* ════════════════════════════════════════════════════════
       READ FORM VALUES
    ════════════════════════════════════════════════════════ */
    function rv(id, fb) {
        var el = document.getElementById(id);
        if (!el) return fb || 0;
        var v = parseFloat(el.value);
        return (isNaN(v) || v <= 0) ? (fb || 0) : v;
    }
    function ri(id, fb) {
        var el = document.getElementById(id);
        if (!el) return fb || 0;
        var v = parseInt(el.value);
        return (isNaN(v) || v < 0) ? (fb || 0) : v;
    }
    function rc(id) { var el = document.getElementById(id); return el ? el.checked : false; }

    function dims() {
        var depth  = rv('txtlenght',  1500);
        var width  = rv('txtwidth',   1500);
        var height = rv('txtheight',  2390);
        var D1=rv('txtD1',1200), D2=rv('txtD2',700), D3=rv('txtD3',800);
        var W1=rv('txtw1',500),  W2=rv('txtw2',500);
        var H1=rv('txtH1',700),  H2=rv('txtH2',1000);
        var EC=rv('txtExtractionChamberHeight',2100);
        var D=rv('txtD', D1+D2+depth);
        var W=rv('txtw', W1+W2+width);
        var H=rv('txtH', H1+H2+height);
        return {
            depth:depth, width:width, height:height,
            D1:D1, D2:D2, D3:D3, W1:W1, W2:W2, H1:H1, H2:H2,
            D:D, W:W, H:H, EC:EC,
            stdD: ri('txtstandardPanels',   Math.floor(D/1140)),
            remD: rv('txtremaingPanels',     D%1140),
            stdW: ri('txtstandardPanelsByW', Math.floor(W/2390)),
            remW: rv('txtremaingPanelsByW',  W%2390),
            stdH: ri('txtstandardPanelsByH', Math.floor(H/2390)),
            remH: rv('txtremaingPanelsByH',  H%2390),
            doorH:   rv('txtHingedDoorHeight', height),
            doorW:   rv('txtHingedDoorWidth',  width),
            hasGlass: rc('chkWatchDoor'),
            glassBot: rv('txtWatchGlassBottomDistance', 400),
            glassH:   rv('txtWatchglassHeight', 400),
            glassW:   rv('WatchglassWidth', 600)
        };
    }

    /* ════════════════════════════════════════════════════════
       REDRAW
    ════════════════════════════════════════════════════════ */
    function redraw() {
        var d = dims();
        var D=d.D, W=d.W, H=d.H, EC=d.EC, D3=d.D3;
        var T=55, ST=70;

        setText('preview3dD', D+' mm');
        setText('preview3dW', W+' mm');
        setText('preview3dH', H+' mm');
        setText('preview3dA', ((W/1000)*(H/1000)).toFixed(2)+' m²');

        if (g) scene.remove(g);
        g = new THREE.Group();

        /* ── 1. Corner columns + perimeter beams ────────────── */
        var colH = H + ST;
        solidBox(ST,colH,ST, C.struct, 0,  colH/2, 0,  1.0);
        solidBox(ST,colH,ST, C.struct, 0,  colH/2, W,  1.0);
        solidBox(ST,colH,ST, C.struct, D,  colH/2, 0,  1.0);
        solidBox(ST,colH,ST, C.struct, D,  colH/2, W,  1.0);
        solidBox(D,ST,ST,   C.struct, D/2, H+ST/2, 0,  1.0);
        solidBox(D,ST,ST,   C.struct, D/2, H+ST/2, W,  1.0);
        solidBox(ST,ST,W,   C.struct, 0,   H+ST/2, W/2,1.0);
        solidBox(ST,ST,W,   C.struct, D,   H+ST/2, W/2,1.0);

        /* ── 2. Side walls ──────────────────────────────────── */
        drawWallPanels(d, T, D, W, H);

        /* ── 3. Ceiling ─────────────────────────────────────── */
        drawCeilingPanels(d, T, D, W, H);

        /* ── 4. Back wall + filter frames ───────────────────── */
        drawBackWall(d, T, ST, D, W, H);

        /* ── 5. Open front sill ─────────────────────────────── */
        solidBox(ST,ST, W+ST*2, C.struct, -ST/2, ST/2, W/2, 1.0);

        /* ── 6. Extraction chamber + transition piece ────────── */
        drawExtractionChamber(d, T, ST, D, W, EC, D3);

        /* ── 7. Viewing glass ───────────────────────────────── */
        if (d.hasGlass) {
            var gW2=d.glassW, gH2=d.glassH, gY=d.glassBot+gH2/2;
            var gMat = wire ? new THREE.MeshBasicMaterial({color:0x88ccff,wireframe:true})
                : new THREE.MeshStandardMaterial({color:0x88ccff,transparent:true,opacity:0.28,roughness:0.05});
            var gm = new THREE.Mesh(new THREE.BoxGeometry(22,gH2,gW2), gMat);
            gm.position.set(-18, gY, W/2); g.add(gm);
            solidBox(22,22,gW2+44,C.door,-18,gY+gH2/2,W/2,1.0);
            solidBox(22,22,gW2+44,C.door,-18,gY-gH2/2,W/2,1.0);
            solidBox(22,gH2+44,22,C.door,-18,gY,W/2-gW2/2,1.0);
            solidBox(22,gH2+44,22,C.door,-18,gY,W/2+gW2/2,1.0);
        }

        /* ── 8. Component placeholder ───────────────────────── */
        var cx=d.D1+d.depth/2, cy=d.height/2, cz=d.W1+d.width/2;
        var cMat = wire ? new THREE.MeshBasicMaterial({color:C.comp,wireframe:true})
            : new THREE.MeshStandardMaterial({color:C.comp,transparent:true,opacity:0.28,roughness:0.5});
        var cm = new THREE.Mesh(new THREE.BoxGeometry(d.depth,d.height,d.width), cMat);
        cm.position.set(cx,cy,cz); g.add(cm);
        edgeBox(d.depth,d.height,d.width, 0xff9040, cx,cy,cz);

        /* ── 9. Dim lines + grid ────────────────────────────── */
        var off=220;
        dimLine(0,-off,W/2,  D,-off,W/2,  0xffffff);
        dimLine(D/2,-off,0,  D/2,-off,W,  0xffffff);
        dimLine(-off,0,0,    -off,H,0,    0xffffff);
        dimLine(D+D3+off,0,W/2, D+D3+off,EC,W/2, 0x40c878);

        //var lblS = Math.max(D + D3, W, H) * 0.09;   // sized relative to the booth
        //dimLabel('D = ' + D + ' mm', D, -off, W / 2, '#ffffff', lblS);  // depth  → back end
        //dimLabel('W = ' + W + ' mm', D / 2, -off, W, '#ffffff', lblS);  // width  → left end
        //dimLabel('H = ' + H + ' mm', -off, H, 0, '#ffffff', lblS);  // height → top corner
        //dimLabel('EC = ' + EC + ' mm', D + D3 + off, EC, W / 2, '#40c878', lblS); // optional EC
        var lblS = Math.max(D + D3, W, H) * 0.08;
        var g0 = 260;   // gap between booth and dimension line

        // D — booth depth: dimension line ABOVE the roof (like D3 above the EC),
        // running along X on the right side (z=0)
        //dimSpan('D = ' + D + ' mm',
        //    0, H + g0, 0, D, H + g0, 0, [0, 1, 0], 0xffffff, lblS);
        dimSpan('D = ' + D + ' mm',
            0, H + g0, W, D, H + g0, W, [0, 1, 0], 0xffffff, lblS);
        
        // W — width of the OPEN FRONT face: dimension line across the opening,
        // at floor level, just in front of the sill
        dimSpan('W = ' + W + ' mm',
            -g0, 0, 0, -g0, 0, W, [-1, 0, 0], 0xffffff, lblS);
        // H — height, vertical, drawn on the front-left corner
        dimSpan('H = ' + H + ' mm',
            -g0, 0, W, -g0, H, W, [-1, 0, 0], 0xffffff, lblS);

        // D3 — the green EC panel span, along X, drawn just above the EC
        dimSpan('D3 = ' + D3 + ' mm',
            D, EC + g0, W, D + D3, EC + g0, W, [0, 1, 0], 0x40c878, lblS);
        // EC — extraction chamber height: vertical span on the right side of the green EC,
        // at the front edge of the chamber (x=D), pushed outward in -Z
        // EC — extraction chamber height: vertical span, pushed well OUT in -Z
        // so it clears the booth and sits beside the green panel
        //dimSpan('EC = ' + EC + ' mm',
        //    D, 0, -g0 * 2.5, D, EC, -g0 * 2.5, [0, 0, -1], 0x40c878, lblS);
        // EC — extraction chamber height, drawn on BOTH side edges so it's
        // visible from either orbit direction
        // EC — extraction chamber height, centred on its vertical line like H
        var ecMat = new THREE.LineBasicMaterial({ color: 0x40c878, transparent: true, opacity: 0.85 });
        g.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints([
            new THREE.Vector3(D, 0, -g0), new THREE.Vector3(D, EC, -g0)]), ecMat));
        // end ticks
        [0, EC].forEach(function (yy) {
            g.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints([
                new THREE.Vector3(D, yy, -g0 - lblS * 0.6),
                new THREE.Vector3(D, yy, -g0 + lblS * 0.6)]), ecMat));
        });
        // label at true centre height, pushed out in -Z
        dimLabel('EC = ' + EC + ' mm', D, EC / 2, -g0 - lblS * 0.9, '#40c878', lblS);


        scene.add(g);
        pivot = {x:(D+D3*0.5)/2, y:H*0.42, z:W/2};
        ang.dist = Math.max(D+D3, W, H)*2.8;
        moveCam();
    }

    /* ════════════════════════════════════════════════════════
       WALL PANELS  (side walls, left Z=W and right Z=0)
       Panels run along X (depth direction)
    ════════════════════════════════════════════════════════ */
    function drawWallPanels(d, T, D, W, H) {
        var panels = buildPanelArray(d.stdD, d.remD, D);
        var xCur = 0;
        for (var i=0; i<panels.length; i++) {
            var pw = panels[i], px = xCur+pw/2;
            var sh = i%2===0 ? C.wall : C.wallAlt;
            panel(pw-8, H, T, sh, px, H/2, 0, 0.72);
            panel(pw-8, H, T, sh, px, H/2, W, 0.72);
            if (i < panels.length-1) {
                seamLine(xCur+pw,0,0,  xCur+pw,H,0);
                seamLine(xCur+pw,0,W,  xCur+pw,H,W);
            }
            xCur += pw;
        }
        if (H>2390) {
            seamLine(0,2390,0, D,2390,0);
            seamLine(0,2390,W, D,2390,W);
        }
    }

    /* ════════════════════════════════════════════════════════
       CEILING PANELS  (along X, span full width Z)
    ════════════════════════════════════════════════════════ */
    function drawCeilingPanels(d, T, D, W, H) {
        var panels = buildPanelArray(d.stdD, d.remD, D);
        var xCur = 0;
        for (var i=0; i<panels.length; i++) {
            var pw=panels[i], px=xCur+pw/2;
            var sh = i%2===0 ? C.wall : C.wallAlt;
            panel(pw-8, T, W, sh, px, H, W/2, 0.68);
            if (i < panels.length-1) seamLine(xCur+pw,H,0, xCur+pw,H,W);
            xCur += pw;
        }
    }

    /* ════════════════════════════════════════════════════════
       BACK WALL + FILTER FRAMES  (panels along Z width)
    ════════════════════════════════════════════════════════ */
    function drawBackWall(d, T, ST, D, W, H) {
        var panels = buildPanelArray(d.stdW, d.remW, W);
        var zCur = 0;
        for (var i=0; i<panels.length; i++) {
            var pw=panels[i], pz=zCur+pw/2;
            var sh = i%2===0 ? C.wall : C.wallAlt;
            panel(T, H, pw-8, sh, D, H/2, pz, 0.55);
            if (i < panels.length-1) seamLine(D,0,zCur+pw, D,H,zCur+pw);
            zCur += pw;
        }
        /* Filter frame grid */
        var fzH=H*0.72, fzW=W-ST*2-20;
        var nW=Math.max(2,Math.round(fzW/540)), nH=Math.max(1,Math.round(fzH/440));
        var fw=fzW/nW, fh=fzH/nH;
        for (var fi=0; fi<nW; fi++)
            for (var fj=0; fj<nH; fj++)
                filterFrame(fw-16, fh-16, ST+10+fw*fi+fw/2, 20+fh*fj+fh/2, D);
    }

    /* ════════════════════════════════════════════════════════
       EXTRACTION CHAMBER

       Key insight from GA3D Image 1:
       The transition piece starts from the FULL TOP of the EC
       (same footprint as the EC box) and tapers to a circle.
       The flat yellow "skin" IS the base of the transition —
       they are seamlessly connected, not a box plus a separate piece.

       Construction:
         - 4 vertical EC side panels + back wall
         - Transition piece = custom BufferGeometry
             base rectangle = full EC top (D3 × W)
             circle top = blower inlet (radius ~W*0.18)
             circle offset toward front face by ~30% of D3
         - Blower cylinder on top of circle
    ════════════════════════════════════════════════════════ */
    function drawExtractionChamber(d, T, ST, D, W, EC, D3) {
        var colS = ST*0.8;

        /* Corner columns */
        solidBox(colS,EC,colS, C.struct, D,    EC/2, 0, 1.0);
        solidBox(colS,EC,colS, C.struct, D,    EC/2, W, 1.0);
        solidBox(colS,EC,colS, C.struct, D+D3, EC/2, 0, 1.0);
        solidBox(colS,EC,colS, C.struct, D+D3, EC/2, W, 1.0);

        /* Top perimeter frame */
        solidBox(D3,colS,colS, C.struct, D+D3/2, EC, 0,   1.0);
        solidBox(D3,colS,colS, C.struct, D+D3/2, EC, W,   1.0);
        solidBox(colS,colS,W,  C.struct, D,      EC, W/2, 1.0);
        solidBox(colS,colS,W,  C.struct, D+D3,   EC, W/2, 1.0);

        /* Side wall panels — right (Z=0) and left (Z=W) */
        var nEC = Math.max(1, Math.round(D3/1140));
        var epW = D3/nEC;
        for (var i=0; i<nEC; i++) {
            var px = D+i*epW+epW/2;
            var sh = i%2===0 ? C.ec : C.ecAlt;
            panel(epW-8, EC, T, sh, px, EC/2, 0, 0.82);
            panel(epW-8, EC, T, sh, px, EC/2, W, 0.82);
            if (i>0) {
                seamLine(D+i*epW,0,0,  D+i*epW,EC,0);
                seamLine(D+i*epW,0,W,  D+i*epW,EC,W);
            }
        }

        /* Back wall panels */
        var nBk = Math.max(1, Math.round(W/1140));
        var bpW = W/nBk;
        for (var j=0; j<nBk; j++) {
            var pz = j*bpW+bpW/2;
            var sh2 = j%2===0 ? C.ec : C.ecAlt;
            panel(T, EC, bpW-8, sh2, D+D3, EC/2, pz, 0.82);
            if (j>0) seamLine(D+D3,0,j*bpW, D+D3,EC,j*bpW);
        }

        /* ────────────────────────────────────────────────────
           TRANSITION PIECE  (the key shape from Image 1)

           BASE rectangle = full EC top surface:
             X: from D to D+D3  (centre = D+D3/2)
             Z: from 0 to W      (centre = W/2)
             Y: EC (top of EC walls)

           CIRCLE top = blower inlet:
             Radius = ~W * 0.18  (roughly 1/3 of width)
             Y = EC + transitionHeight
             X centre = D + D3*0.35  (offset towards FRONT,
               matching Image 1 where circle is front-left)
             Z centre = W/2 (centred on width)

           The surface is ONE PIECE — base fills entire EC top
           and sweeps up to the circle outlet.
        ──────────────────────────────────────────────────── */
        var topY    = EC;                       // Y of the rectangular base
        var transH  = Math.min(D3*0.55, W*0.30, 420);  // height of taper
        var circY   = EC + transH;

        /* Base rectangle — full EC footprint */
        var bX0 = D,     bX1 = D+D3;           // X extents
        var bZ0 = 0,     bZ1 = W;              // Z extents
        var bCX = D+D3/2,  bCZ = W/2;          // base centre

        /* Circle — offset towards front (lower X = closer to booth) */
        var circR  = Math.min(W*0.18, D3*0.40, 300);
        var circX  = D + D3*0.38;              // offset ~38% from booth back = front-biased
        var circZ  = W/2;

        drawSquareToRoundTransition(
            bX0, bX1, bZ0, bZ1, topY,          // rectangular base extents + Y
            circX, circY, circZ, circR,          // circle centre + radius
            32                                  // segments for smooth circle
        );

        
    }

    /* ════════════════════════════════════════════════════════
       SQUARE-TO-ROUND TRANSITION PIECE
       ─────────────────────────────────────────────────────
       This function builds the exact shape in GA3D Image 1.

       The shape has:
       ① A flat rectangular base (full EC top, a quadrilateral)
       ② 4 curved/tapered side surfaces (each a ruled surface
          from a straight bottom edge to a curved arc at top)
       ③ A circular top cap (flat disk = blower inlet flange)

       Parameters:
         bX0, bX1, bZ0, bZ1, bY  — rectangular BASE extents + height
         cX, cY, cZ, cR          — circle CENTRE and radius (top)
         segs                    — circle subdivisions (32 recommended)

       Algorithm (per-side, using "ruled surface" approach):
         - Divide the 4 base edges into segs total points
         - Map each base perimeter point to the nearest circle point
         - This creates the smooth diagonal taper seen in Image 1
    ════════════════════════════════════════════════════════ */
    function drawSquareToRoundTransition(bX0, bX1, bZ0, bZ1, bY, cX, cY, cZ, cR, segs) {

        var verts = [];   // position buffer

        function tri(A, B, C_p) {
            verts.push(A[0],A[1],A[2], B[0],B[1],B[2], C_p[0],C_p[1],C_p[2]);
        }

        /* ── Generate circle points at top ─────────────────── */
        var tc = [];
        for (var i=0; i<segs; i++) {
            var a = (i/segs)*Math.PI*2;
            tc.push([ cX+Math.cos(a)*cR, cY, cZ+Math.sin(a)*cR ]);
        }

        /* ── Generate base perimeter points ─────────────────
           Walk the rectangle CCW (when viewed from above):
           Right side (+X edge):  Z0→Z1 at X=bX1
           Back side  (+Z edge):  X1→X0 at Z=bZ1
           Left side  (-X edge):  Z1→Z0 at X=bX0
           Front side (-Z edge):  X0→X1 at Z=bZ0
           Total = segs points equally spaced around perimeter
        ─────────────────────────────────────────────────── */
        var perimeter = 2*(bX1-bX0) + 2*(bZ1-bZ0);
        var sideLen   = [bZ1-bZ0, bX1-bX0, bZ1-bZ0, bX1-bX0];
        var sideSegs  = sideLen.map(function(l){ return Math.max(1,Math.round(segs*l/perimeter)); });

        // Adjust so total = segs
        var total = sideSegs.reduce(function(a,b){return a+b;},0);
        sideSegs[0] += segs - total;   // absorb rounding error into side 0

        var bp = [];   // base perimeter points (world coords)

        // Side 0: right edge, Z0→Z1, X=bX1
        for (var k=0; k<sideSegs[0]; k++) {
            var t = k/sideSegs[0];
            bp.push([ bX1, bY, bZ0 + (bZ1-bZ0)*t ]);
        }
        // Side 1: back edge, X1→X0, Z=bZ1
        for (var k=0; k<sideSegs[1]; k++) {
            var t = k/sideSegs[1];
            bp.push([ bX1 + (bX0-bX1)*t, bY, bZ1 ]);
        }
        // Side 2: left edge, Z1→Z0, X=bX0
        for (var k=0; k<sideSegs[2]; k++) {
            var t = k/sideSegs[2];
            bp.push([ bX0, bY, bZ1 + (bZ0-bZ1)*t ]);
        }
        // Side 3: front edge, X0→X1, Z=bZ0
        for (var k=0; k<sideSegs[3]; k++) {
            var t = k/sideSegs[3];
            bp.push([ bX0 + (bX1-bX0)*t, bY, bZ0 ]);
        }

        /* ── Lateral surface: connect bp[i]→bp[i+1] to tc[i]→tc[i+1] ──
           Each base perimeter point maps to the SAME-INDEX circle point.
           This is the standard square-to-round ruled surface.
        ─────────────────────────────────────────────────────────── */
        var n = bp.length;   // should equal segs
        for (var i=0; i<n; i++) {
            var j = (i+1)%n;
            var bA = bp[i], bB = bp[j];
            var tA = tc[i], tB = tc[j];
            // Quad → 2 triangles (wound outward)
            tri(bA, bB, tA);
            tri(bB, tB, tA);
        }

        /* ── Circular top cap ────────────────────────────── */
        var topC = [cX, cY, cZ];
        for (var i=0; i<segs; i++) {
            tri(topC, tc[(i+1)%segs], tc[i]);
        }

        /* ── Rectangular bottom cap ──────────────────────── */
        // Two triangles covering the full base rectangle
        var A=[bX0,bY,bZ0], B=[bX1,bY,bZ0], Cp=[bX1,bY,bZ1], Dp=[bX0,bY,bZ1];
        tri(A, Cp, B);
        tri(A, Dp, Cp);

        /* ── Build BufferGeometry ────────────────────────── */
        var geo = new THREE.BufferGeometry();
        geo.setAttribute('position',
            new THREE.Float32BufferAttribute(new Float32Array(verts), 3));
        geo.computeVertexNormals();

        /* Solid face material — yellow-green matching GA3D */
        var mat = wire
            ? new THREE.MeshBasicMaterial({ color:C.trans, wireframe:true })
            : new THREE.MeshStandardMaterial({
                color: C.trans,
                roughness: 0.22,
                metalness: 0.55,
                side: THREE.DoubleSide
              });

        g.add(new THREE.Mesh(geo, mat));

        /* Bright edge outline so the shape reads clearly */
        var wMat = new THREE.LineBasicMaterial({
            color: 0xeef060, opacity: 0.70, transparent: true
        });
        g.add(new THREE.LineSegments(new THREE.WireframeGeometry(geo), wMat));
    }

    /* ════════════════════════════════════════════════════════
       PANEL ARRAY BUILDER
    ════════════════════════════════════════════════════════ */
    function buildPanelArray(stdCount, rem, total) {
        var panels = [];
        if (stdCount > 0) {
            var sw = (total-(rem>10?rem:0))/stdCount;
            for (var i=0; i<stdCount; i++) panels.push(sw);
        }
        if (rem>10) panels.push(rem);
        if (panels.length===0) panels=[total];
        return panels;
    }

    /* ════════════════════════════════════════════════════════
       GEOMETRY HELPERS
    ════════════════════════════════════════════════════════ */
    function solidBox(w,h,d,color,px,py,pz,opacity) {
        var mat = wire
            ? new THREE.MeshBasicMaterial({color:color,wireframe:true})
            : new THREE.MeshStandardMaterial({
                color:color,roughness:0.4,metalness:0.45,
                transparent:opacity<1, opacity:opacity!==undefined?opacity:1.0
            });
        var m = new THREE.Mesh(new THREE.BoxGeometry(w,h,d), mat);
        m.position.set(px,py,pz); g.add(m);
    }

    function panel(w,h,d,color,px,py,pz,opacity) {
        var mat = wire
            ? new THREE.MeshBasicMaterial({color:color,wireframe:true})
            : new THREE.MeshStandardMaterial({
                color:color,roughness:0.35,metalness:0.3,
                transparent:true,opacity:opacity||0.72,side:THREE.DoubleSide
            });
        var m = new THREE.Mesh(new THREE.BoxGeometry(w,h,d), mat);
        m.position.set(px,py,pz); g.add(m);
        edgeBox(w,h,d, lighten(color,55), px,py,pz);
    }

    function filterFrame(fw,fh,pz,py,px) {
        var mat = wire
            ? new THREE.MeshBasicMaterial({color:C.filter,wireframe:true})
            : new THREE.MeshStandardMaterial({color:C.filter,roughness:0.75,transparent:true,opacity:0.88});
        var m = new THREE.Mesh(new THREE.BoxGeometry(26,fh,fw), mat);
        m.position.set(px-18,py,pz); g.add(m);
        edgeBox(26,fh,fw, 0xf0c060, px-18,py,pz);
    }

    function edgeBox(w,h,d,color,px,py,pz) {
        var ls = new THREE.LineSegments(
            new THREE.EdgesGeometry(new THREE.BoxGeometry(w,h,d)),
            new THREE.LineBasicMaterial({color:color,opacity:0.52,transparent:true})
        );
        ls.position.set(px,py,pz); g.add(ls);
    }

    function seamLine(x1,y1,z1,x2,y2,z2) {
        var geo = new THREE.BufferGeometry().setFromPoints([
            new THREE.Vector3(x1,y1,z1), new THREE.Vector3(x2,y2,z2)]);
        g.add(new THREE.Line(geo,
            new THREE.LineBasicMaterial({color:C.seam,opacity:0.9,transparent:true})));
    }

    function dimLine(x1,y1,z1,x2,y2,z2,color) {
        var geo = new THREE.BufferGeometry().setFromPoints([
            new THREE.Vector3(x1,y1,z1), new THREE.Vector3(x2,y2,z2)]);
        g.add(new THREE.Line(geo,
            new THREE.LineBasicMaterial({color:color,opacity:0.30,transparent:true})));
    }

    /* ════════════════════════════════════════════════════════
       CAMERA
    ════════════════════════════════════════════════════════ */
    function moveCam() {
        var th=ang.th, ph=ang.ph, r=ang.dist;
        cam.position.set(
            pivot.x + r*Math.sin(ph)*Math.sin(th),
            pivot.y + r*Math.cos(ph),
            pivot.z + r*Math.sin(ph)*Math.cos(th)
        );
        cam.lookAt(pivot.x, pivot.y, pivot.z);
    }



    function roundRect(ctx, x, y, w, h, r) {
        ctx.beginPath();
        ctx.moveTo(x + r, y);
        ctx.arcTo(x + w, y, x + w, y + h, r);
        ctx.arcTo(x + w, y + h, x, y + h, r);
        ctx.arcTo(x, y + h, x, y, r);
        ctx.arcTo(x, y, x + w, y, r);
        ctx.closePath();
    }

    function dimLabel(text, x, y, z, color, scale) {
        var cv = document.createElement('canvas');
        cv.width = 512; cv.height = 160;
        var ctx = cv.getContext('2d');

        ctx.fillStyle = 'rgba(13,17,23,0.82)';
        roundRect(ctx, 4, 4, cv.width - 8, cv.height - 8, 40);
        ctx.fill();
        ctx.lineWidth = 5; ctx.strokeStyle = color; ctx.stroke();

        ctx.font = 'bold 66px Segoe UI, Arial, sans-serif';
        ctx.fillStyle = color;
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(text, cv.width / 2, cv.height / 2 + 4);

        var tex = new THREE.CanvasTexture(cv);
        tex.minFilter = THREE.LinearFilter;

        var sp = new THREE.Sprite(new THREE.SpriteMaterial({
            map: tex, depthTest: false, depthWrite: false, transparent: true
        }));
        sp.scale.set(scale * (cv.width / cv.height), scale, 1);
        sp.position.set(x, y, z);
        sp.renderOrder = 999;      // draw on top of walls
        g.add(sp);
    }
    // Draws a dimension line from A→B, with short end-ticks, and the value
    // label centred on the span. `tickDir` is a unit-ish [x,y,z] the ticks
    // point along (usually perpendicular to the run).
    function dimSpan(text, ax, ay, az, bx, by, bz, tickDir, color, lblScale) {
        var A = new THREE.Vector3(ax, ay, az);
        var B = new THREE.Vector3(bx, by, bz);

        // main dimension line
        var mat = new THREE.LineBasicMaterial({ color: color, transparent: true, opacity: 0.85 });
        g.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints([A, B]), mat));

        // end ticks
        var tl = Math.max(ax, ay, az, bx, by, bz, 1) * 0; // placeholder
        var tick = new THREE.Vector3(tickDir[0], tickDir[1], tickDir[2]).normalize()
            .multiplyScalar(lblScale * 0.6);
        [A, B].forEach(function (P) {
            var t1 = P.clone().add(tick), t2 = P.clone().sub(tick);
            g.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints([t1, t2]), mat));
        });

        // label at midpoint, pushed out along the tick direction so it clears the line
        var mid = A.clone().add(B).multiplyScalar(0.5)
            .add(new THREE.Vector3(tickDir[0], tickDir[1], tickDir[2]).normalize()
                .multiplyScalar(lblScale * 0.9));
        dimLabel(text, mid.x, mid.y, mid.z,
            '#' + color.toString(16).padStart(6, '0'), lblScale);
    }
    /* ════════════════════════════════════════════════════════
       UTILITIES
    ════════════════════════════════════════════════════════ */
    function lighten(hex,a) {
        return (Math.min(255,((hex>>16)&0xff)+a)<<16)|
               (Math.min(255,((hex>>8) &0xff)+a)<<8 )|
               (Math.min(255,( hex     &0xff)+a));
    }
    function clamp(v,lo,hi) { return Math.max(lo,Math.min(hi,v)); }
    function setText(id,t)  { var el=document.getElementById(id); if(el) el.textContent=t; }
    function addBtn(id,fn)  { var el=document.getElementById(id); if(el) el.addEventListener('click',fn); }

}());
