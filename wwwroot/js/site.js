'use strict';
// Anclar acciones de la aplicación a formularios y enlaces reales.
// Esc permite cancelar cualquier alerta de formulario antes de hacer otra operación.
document.addEventListener('keydown', e => {
 if (e.key === 'Escape') {
   document.querySelectorAll('.inline-error').forEach(n => n.hidden = true);
 }
});
