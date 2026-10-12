// ============================================================
//  How to integrate 3D preview into Create.cshtml
// ============================================================

// ── STEP 1: Add Three.js CDN in @section Scripts ─────────────
// Add this BEFORE your other scripts in @section Scripts:

@section Scripts {

    @* Three.js for 3D preview *@
    <script src="https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js"></script>

    @* Existing scripts *@
    <script src="~/js/paintbooth.js" asp-append-version="true"></script>

    @* 3D preview — load AFTER three.js *@
    <script src="~/js/paintbooth3d.js" asp-append-version="true"></script>
}


// ── STEP 2: Add canvas section in Create.cshtml ──────────────
// Paste the contents of 3DPreview_Snippet.cshtml
// after Section 3 (PaintBooth Dimensions) in your Create.cshtml


// ── STEP 3: Add CSS to paintbooth.css ────────────────────────

.pb-dim-preview-card {
    background    : var(--bg-hover);
    border        : 1px solid var(--border);
    border-radius : 8px;
    padding       : 10px 12px;
}


// ── HOW IT WORKS ─────────────────────────────────────────────
// paintbooth3d.js reads these existing form input IDs:
//   txtD1, txtD2, txtD3  → D calculation
//   txtw1, txtw2         → W calculation
//   txtH1, txtH2         → H calculation
//   txtlenght            → Component depth (from enquiry)
//   txtwidth             → Component width (from enquiry)
//   txtheight            → Component height (from enquiry)
//   txtExtractionChamberHeight → EC height
//   txtD, txtw, txtH     → Auto-calculated totals
//
// Every time any input changes, the 3D preview rebuilds automatically.
// No changes needed to paintbooth.js — the 3D script is standalone.


// ── COLOUR CODING IN 3D VIEW ─────────────────────────────────
// Blue  (#2a7fff) — PaintBooth wall panels (D × H, D × W sides)
// Green (#1db87a) — Extraction Chamber panels (D3 sides, back wall)
// Amber (#ff9800) — Component box (the part being painted, inside booth)
// White lines    — Dimension reference lines (D, W, H axes)


// ── CONTROLS ─────────────────────────────────────────────────
// Mouse drag    → Orbit camera around paintbooth
// Mouse scroll  → Zoom in / out
// Reset button  → Snap camera back to default angle
// Wireframe btn → Toggle between solid and wireframe view
