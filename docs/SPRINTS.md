# Backlog y Planificación de Sprints

Proyecto: **Sistema de Simulación de Créditos**
Marco de trabajo: adaptación de Scrum a micro-sprints (3 sprints × 5 días = 15 días)

---

## Product Backlog — Historias de Usuario

| ID | Historia de usuario | Criterios de aceptación | Sprint |
|---|---|---|---|
| HU-01 | Como visitante, quiero **registrarme** con correo y contraseña para tener una cuenta en el simulador. | El correo es único; la contraseña se guarda con hash (nunca en texto plano); se rechazan correos mal formados. | 1 |
| HU-02 | Como usuario registrado, quiero **iniciar sesión** para obtener un token que me dé acceso al simulador. | Credenciales válidas devuelven un JWT firmado con expiración; credenciales inválidas devuelven 401 sin revelar cuál dato falló. | 1 |
| HU-03 | Como usuario, quiero una **pantalla de login** clara para acceder sin fricción. | Formulario con validación en vivo, mensajes de error legibles y persistencia del token en el navegador. | 1 |
| HU-04 | Como usuario autenticado, quiero **consultar los tipos de crédito** disponibles y su tasa para elegir con información. | El endpoint devuelve los tres tipos con su tasa anual leída de la base de datos. | 2 |
| HU-05 | Como usuario autenticado, quiero **simular un crédito** indicando monto, plazo y tipo para conocer mis cuotas. | La tasa se deriva del tipo elegido; se devuelven las tablas francesa y alemana en un único JSON. | 2 |
| HU-06 | Como responsable del sistema, quiero que **los endpoints de crédito exijan JWT** para que nadie simule sin autenticarse. | Una petición sin token o con token vencido devuelve 401. | 2 |
| HU-07 | Como usuario, quiero un **formulario de simulación** cómodo para lanzar el cálculo. | Validación de monto y plazo en rangos permitidos, con mensajes en español. | 3 |
| HU-08 | Como usuario, quiero **ver las dos tablas de amortización** lado a lado para compararlas. | Tablas con cuota, interés, capital y saldo por período, más totales, en formato de moneda. | 3 |
| HU-09 | Como usuario, quiero una **interfaz atractiva y comprensible** para usar el simulador con confianza. | Diseño responsive, estados de carga y error, y coherencia visual en todas las pantallas. | 3 |

---

## Sprint 1 — Cimientos y Autenticación (Días 1 a 5)

**Objetivo del sprint:** que el sistema permita registro y acceso seguro con JWT almacenado.

Historias: HU-01, HU-02, HU-03

Tareas técnicas:
- [ ] Instalar y configurar PostgreSQL 18; crear `authdb` y `creditdb`.
- [ ] Crear la solución .NET 10 y el proyecto `Auth.Api`.
- [ ] Modelar la entidad `Usuario` y generar la migración inicial con EF Core.
- [ ] Implementar hash de contraseñas con BCrypt.
- [ ] Implementar `POST /api/auth/register` y `POST /api/auth/login` con emisión de JWT.
- [ ] Configurar CORS para la SPA.
- [ ] Crear el frontend con Vite + React + TypeScript + Tailwind.
- [ ] Implementar la vista de Login y el almacenamiento del token.

**Incremento entregable:** un usuario puede registrarse, iniciar sesión y conservar su sesión.

---

## Sprint 2 — Lógica de Simulación (Días 6 a 10)

**Objetivo del sprint:** API capaz de recibir datos y retornar tablas matemáticamente exactas.

Historias: HU-04, HU-05, HU-06

Tareas técnicas:
- [ ] Crear el proyecto `Credit.Api` y su contexto sobre `creditdb`.
- [ ] Sembrar el catálogo de tipos de crédito con sus tasas (15.50 / 8.50 / 22.00).
- [ ] Implementar la regla: tipo de crédito → tasa de interés.
- [ ] Programar el algoritmo de amortización francés.
- [ ] Programar el algoritmo de amortización alemán.
- [ ] Validar la exactitud con pruebas unitarias (xUnit).
- [ ] Proteger los endpoints exigiendo el JWT emitido por Auth API.
- [ ] Persistir el historial de simulaciones del usuario.

**Incremento entregable:** endpoints de simulación funcionales y verificados.

---

## Sprint 3 — Interfaz Atractiva y Cierre (Días 11 a 15)

**Objetivo del sprint:** plataforma finalizada, interactiva y lista para revisión del usuario final.

Historias: HU-07, HU-08, HU-09

Tareas técnicas:
- [ ] Diseñar el formulario de simulación en React.
- [ ] Renderizar ambas tablas de amortización devueltas por el API.
- [ ] Mostrar el resumen comparativo (cuota inicial, total de intereses, total pagado).
- [ ] Aplicar validaciones de errores de usuario y estados de carga.
- [ ] Pulido visual y responsive.
- [ ] Documentar la puesta en marcha en el README.

**Incremento entregable:** producto completo listo para demostración.

---

## Registro de evidencias

Se completa al cierre de cada sprint.

| Sprint | Fecha de cierre | Incremento demostrado | Adaptaciones realizadas |
|---|---|---|---|
| 1 | | | |
| 2 | | | |
| 3 | | | |
