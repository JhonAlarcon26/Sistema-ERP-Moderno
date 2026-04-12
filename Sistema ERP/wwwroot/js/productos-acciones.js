
function verDetalleProducto(id) {
    fetch('/Productos/EditarProducto/' + id)
        .then(response =\u003e response.text())
        .then(html =\u003e {
            document.getElementById('detalleProductoContainer').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('detalleProductoModal'));
            modal.show();
        })
        .catch(err =\u003e console.error('Error al cargar detalle:', err));
}

function previewProducto(btn) {
    const id = btn.getAttribute('data-id');
    const nom = btn.getAttribute('data-nombre');
    const cat = btn.getAttribute('data-categoria');
    const pc = btn.getAttribute('data-preciocompra');
    const pi = btn.getAttribute('data-preciointerno');
    const ps = btn.getAttribute('data-precioventa');
    const st = btn.getAttribute('data-stock');
    const sn = btn.getAttribute('data-sn');
    const desc = btn.getAttribute('data-descripcion');
    const img = btn.getAttribute('data-imagen');

    document.getElementById('prvNombre').innerText = nom || "Sin Nombre";
    document.getElementById('prvCategoria').innerText = cat || "Sin Categoría";
    document.getElementById('prvPrecioCompra').innerText = pc || "0.00";
    document.getElementById('prvPrecioInterno').innerText = pi || "0.00";
    document.getElementById('prvPrecioVenta').innerText = ps || "0.00";
    document.getElementById('prvStock').innerText = st || "0";
    document.getElementById('prvCodigo').innerText = sn || "---";
    document.getElementById('prvDescripcion').innerText = desc || "Sin descripción adicional.";
    
    const imgEl = document.getElementById('prvImagen');
    if (img \u0026\u0026 img !== \"\") {
        imgEl.src = img;
        imgEl.classList.remove('hidden');
    } else {
        imgEl.src = \"\";
        imgEl.classList.add('hidden');
    }

    const modalEl = document.getElementById('modalPreviewProducto');
    if (modalEl) {
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    } else {
        alert('Error: No se encontró el modal de previsualización.');
    }
}

function imprimirFichaProducto() {
    const nom = document.getElementById('prvNombre').innerText;
    const cat = document.getElementById('prvCategoria').innerText;
    const sn = document.getElementById('prvCodigo').innerText;
    const pc = document.getElementById('prvPrecioCompra').innerText;
    const pi = document.getElementById('prvPrecioInterno').innerText;
    const ps = document.getElementById('prvPrecioVenta').innerText;
    const st = document.getElementById('prvStock').innerText;
    const desc = document.getElementById('prvDescripcion').innerText;

    let html = '\u003ch1\u003eFicha Técnica de Producto\u003c/h1\u003e';
    html += '\u003cp\u003e\u003cstrong\u003e' + nom + '\u003c/strong\u003e\u003c/p\u003e';
    html += '\u003ctable\u003e';
    html += '\u003ctr\u003e\u003ctd class=\"lbl\"\u003eCategoría\u003c/td\u003e\u003ctd class=\"val\"\u003e' + cat + '\u003c/td\u003e\u003c/tr\u003e';
    html += '\u003ctr\u003e\u003ctd class=\"lbl\"\u003eCódigo SN\u003c/td\u003e\u003ctd class=\"val\"\u003e' + sn + '\u003c/td\u003e\u003c/tr\u003e';
    html += '\u003ctr\u003e\u003ctd class=\"lbl\"\u003eStock Actual\u003c/td\u003e\u003ctd class=\"val\"\u003e' + st + '\u003c/td\u003e\u003c/tr\u003e';
    html += '\u003ctr\u003e\u003ctd class=\"lbl\"\u003ePrecio Compra\u003c/td\u003e\u003ctd class=\"val\"\u003eBs. ' + pc + '\u003c/td\u003e\u003c/tr\u003e';
    html += '\u003ctr\u003e\u003ctd class=\"lbl\"\u003ePrecio Venta\u003c/td\u003e\u003ctd class=\"val\"\u003eBs. ' + ps + '\u003c/td\u003e\u003c/tr\u003e';
    html += '\u003c/table\u003e';
    if (desc \u0026\u0026 desc !== \"Sin descripción adicional.\") {
        html += '\u003ch2\u003eDescripción\u003c/h2\u003e';
        html += '\u003cp\u003e' + desc + '\u003c/p\u003e';
    }

    const win = window.open('', '_blank');
    win.document.write('\u003chtml\u003e\u003chead\u003e\u003ctitle\u003eImprimir Ficha\u003c/title\u003e\u003cstyle\u003e');
    win.document.write('body{font-family:sans-serif;padding:40px;color:#333} h1{color:#1e293b;border-bottom:2px solid #e2e8f0;padding-bottom:10px} table{width:100%;margin-top:20px} td{padding:10px;border-bottom:1px solid #f1f5f9} .lbl{font-weight:bold;color:#64748b;width:30%} .val{color:#1e293b} .footer{margin-top:50px;font-size:10px;color:#94a3b8;border-top:1px solid #e2e8f0;padding-top:10px;text-align:center}');
    win.document.write('\u003c/style\u003e\u003c/head\u003e\u003cbody\u003e');
    win.document.write(html);
    win.document.write('\u003cdiv class=\"footer\"\u003eDocumento generado automáticamente por STC Services · No válido como comprobante fiscal\u003c/div\u003e');
    win.document.write('\u003c/body\u003e\u003c/html\u003e');
    win.document.close();
    win.print();
}
