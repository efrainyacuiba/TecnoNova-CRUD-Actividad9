'use strict';
(() => {
const tabla = document.getElementById('lineas');
if (!tabla) return;
const $ = id => document.getElementById(id);
const items = [];
let registrada = false;
const money = v => 'Bs ' + (Math.round((v + Number.EPSILON) * 100) / 100).toLocaleString('en-US', {minimumFractionDigits: 2, maximumFractionDigits: 2});
const esc = t => String(t).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
function aviso(id, texto) { const n=$(id); n.textContent=texto; n.hidden=!texto; }
function render() {
  let bruto=0;
  if(!items.length) tabla.innerHTML='<tr><td colspan="5" class="empty">El carrito está vacío. Agrega al menos un artículo.</td></tr>';
  else tabla.innerHTML=items.map((d,i)=>{
    bruto += d.cantidad*d.precio;
    return `<tr><td>${esc(d.nombre)}</td><td>${d.cantidad}</td><td>${money(d.precio)}</td><td><strong>${money(d.cantidad*d.precio)}</strong></td><td><button type="button" class="mini danger" data-borrar="${i}">Quitar</button></td></tr>`;
  }).join('');
  const porcentaje=Number($('descuento').value);
  const invalido = !Number.isFinite(porcentaje) || porcentaje < 0 || porcentaje > 20;
  aviso('aviso-descuento', invalido ? 'El descuento debe estar entre 0 % y 20 %. Cambia el porcentaje para continuar.' : '');
  const monto=Math.round((bruto*porcentaje/100 + Number.EPSILON)*100)/100;
  $('subtotal').textContent=money(bruto);
  $('monto').textContent=invalido ? 'No válido' : '− '+money(monto);
  $('total').textContent=invalido ? 'Corrige el porcentaje' : money(bruto-monto);
  $('descuento').classList.toggle('invalid',invalido);
}
$('agregar').addEventListener('click',()=>{
  if (registrada) return aviso('aviso-articulo','Esta venta ya fue guardada. Pulsa Vaciar carrito para comenzar una nueva.');
  const opcion=$('producto').selectedOptions[0];
  const cantidad=Number($('cantidad').value);
  if (!opcion || !opcion.value) return aviso('aviso-articulo','Elige un producto antes de añadirlo.');
  if (!Number.isInteger(cantidad) || cantidad<=0) return aviso('aviso-articulo','Utiliza una cantidad entera mayor que cero.');
  const stock=Number(opcion.dataset.stock);
  const existente=items.find(i=>i.idProducto===Number(opcion.value));
  if (cantidad+(existente?.cantidad||0)>stock) return aviso('aviso-articulo',`No hay suficiente stock. Disponible: ${stock} unidades.`);
  if (existente) existente.cantidad+=cantidad;
  else items.push({idProducto:Number(opcion.value),nombre:opcion.dataset.nombre,precio:Number(opcion.dataset.precio),cantidad});
  aviso('aviso-articulo',''); aviso('mensaje-venta','');render();
});
tabla.addEventListener('click',e=>{ const b=e.target.closest('button[data-borrar]'); if(!b)return; if(registrada) return aviso('aviso-articulo','Esta venta ya fue guardada. Pulsa Vaciar carrito para comenzar una nueva.'); items.splice(Number(b.dataset.borrar),1);render(); });
$('vaciar').addEventListener('click',()=>{ if (registrada) { window.location.reload(); return; } items.length=0; registrada=false; $('confirmar').disabled=false; render(); aviso('mensaje-venta',''); aviso('aviso-articulo',''); });
$('descuento').addEventListener('input',render);
document.addEventListener('keydown',e=>{
  if (e.key==='Escape' && document.activeElement!==$('confirmar')) { aviso('mensaje-venta',''); aviso('aviso-articulo',''); }
  if (e.ctrlKey && e.key.toLowerCase()==='s') {e.preventDefault();$('form-venta').requestSubmit();}
});
$('form-venta').addEventListener('submit',async(e)=>{
 e.preventDefault(); if(registrada)return; const boton=$('confirmar'); boton.disabled=true;
 try {
  const body={cliente:$('cliente').value,vendedor:$('vendedor').value,
       porcentajeDescuento:Number($('descuento').value),
       items:items.map(({idProducto,cantidad})=>({idProducto,cantidad}))};
  // El servidor vuelve a comprobar descuentos, stock y existencia de artículos.
  const token=document.querySelector('#form-venta input[name="__RequestVerificationToken"]').value;
  const r=await fetch('/Ventas/Registrar',{method:'POST',headers:{'Content-Type':'application/json','RequestVerificationToken':token},body:JSON.stringify(body)});
  const data=await r.json();
  const n=$('mensaje-venta'); n.className='notice '+(r.ok?'success':'error');
  n.textContent=data.mensaje||(r.ok?'Venta registrada.':'No se pudo registrar la venta.'); n.hidden=false;
  n.scrollIntoView({behavior:'smooth',block:'nearest'});
  if(r.ok) { registrada=true; } // Conservar los artículos y el aviso para captura A8.
 } catch (err) {
   const n=$('mensaje-venta'); n.className='notice error';
   n.textContent='No pudimos contactar con la aplicación. Verifique la conexión y vuelva a intentarlo.';n.hidden=false;
 } finally { boton.disabled=registrada; }
});
render();
})();
