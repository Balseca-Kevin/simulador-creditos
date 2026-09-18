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
- [x] Configurar PostgreSQL y crear `authdb`.
- [x] Crear la solución .NET 10 y el proyecto `Auth.Api`.
- [x] Modelar la entidad `Usuario` y generar la migración inicial con EF Core.
- [x] Implementar hash de contraseñas con BCrypt.
- [x] Implementar `POST /api/auth/register` y `POST /api/auth/login` con emisión de JWT.
- [x] Configurar CORS para la SPA.
- [x] Crear el frontend con Vite + React + TypeScript + Tailwind.
- [x] Implementar la vista de Login y el almacenamiento del token.

**Incremento entregable:** un usuario puede registrarse, iniciar sesión y conservar su sesión.

### Pruebas de aceptación ejecutadas al cierre

| # | Caso | Resultado esperado | Obtenido |
|---|---|---|---|
| 1 | `GET /health` | Servicio activo | ✅ 200 |
| 2 | Registro de usuario nuevo | 201 con JWT y perfil | ✅ token de 435 caracteres |
| 3 | Registro con correo repetido | 409 y mensaje explicativo | ✅ 409 |
| 4 | Login con contraseña incorrecta | 401 sin revelar qué dato falló | ✅ 401 |
| 5 | Login correcto | 200 con JWT vigente | ✅ 200 |
| 6 | `GET /api/auth/me` con token | Perfil del portador | ✅ 200 |
| 7 | `GET /api/auth/me` sin token | 401 | ✅ 401 |
| 8 | `GET /api/auth/me` con firma alterada | 401 | ✅ 401 |
| 9 | Registro con datos inválidos | 400 con mensajes en español | ✅ 400 |
| 10 | Preflight CORS desde `localhost:5173` | Origen permitido | ✅ cabeceras emitidas |
| 11 | Preflight CORS desde origen ajeno | Sin cabeceras `Access-Control` | ✅ rechazado |
| 12 | Contraseña almacenada en la base | Hash BCrypt, nunca texto plano | ✅ `$2a$11$…` |

---

## Sprint 2 — Lógica de Simulación (Días 6 a 10)

**Objetivo del sprint:** API capaz de recibir datos y retornar tablas matemáticamente exactas.

Historias: HU-04, HU-05, HU-06

Tareas técnicas:
- [x] Crear el proyecto `Credit.Api` y su contexto sobre `creditdb`.
- [x] Sembrar el catálogo de tipos de crédito con sus tasas (15.50 / 8.50 / 22.00).
- [x] Implementar la regla: tipo de crédito → tasa de interés.
- [x] Programar el algoritmo de amortización francés.
- [x] Programar el algoritmo de amortización alemán.
- [x] Validar la exactitud con pruebas unitarias (xUnit).
- [x] Proteger los endpoints exigiendo el JWT emitido por Auth API.
- [x] Persistir el historial de simulaciones del usuario.

**Incremento entregable:** endpoints de simulación funcionales y verificados.

### Endpoints entregados (Credit API, puerto 5090)

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/creditos/tipos` | Catálogo de tipos de crédito con su tasa vigente |
| `POST` | `/api/creditos/simular` | Recibe tipo, monto y plazo; devuelve tablas francesa y alemana |
| `GET` | `/api/creditos/historial` | Últimas 50 simulaciones del usuario autenticado |

Todos exigen `Authorization: Bearer <JWT>` emitido por la Auth API.

### Pruebas unitarias: 42 en verde

| Grupo | Qué verifica |
|---|---|
| Método francés | Cuota de referencia de manual (10 000 al 12 % a 12 meses = 888.49), cuota idéntica en todas las filas, interés decreciente y capital creciente, crédito sin interés |
| Método alemán | Caso calculado a mano (12 000 al 12 %: 1 120 → 1 010, interés total 780), capital constante, cuota decreciente, menor interés total que el francés |
| Integridad (6 escenarios × 2 métodos) | Suma de capital = monto exacto, saldo final 0, cada fila cuadra, saldos encadenados, cuota fija (francés) y capital fijo (alemán) al centavo |
| Regla de negocio | Conversión a tasa mensual de las 3 tasas, mayor tasa = mayor costo, rechazo de montos, plazos y tasas inválidos |

### Defecto detectado y corregido durante el sprint

La primera versión del motor redondeaba la cuota francesa a centavos y luego
iteraba sobre el saldo redondeado. La fracción de centavo que se dejaba de pagar
cada mes se **capitalizaba con (1+i)ⁿ**: en un crédito de 100 000 al 22 % a 480
meses, la última "cuota fija" salía en **2 693.27 frente a 1 833.63**, 859.64 de
desviación. Las pruebas de sumas y saldos pasaban igual, porque la última fila
absorbía la diferencia.

Lo detectó la prueba que verifica la propiedad que *define* al método francés, y
no solo sus totales. Corrección: cada fila se ancla al saldo teórico exacto,
calculado con precisión decimal completa, y el error de redondeo nunca supera un
centavo ni se acumula. El caso quedó como prueba de regresión.

### Pruebas de aceptación ejecutadas al cierre

| # | Caso | Resultado esperado | Obtenido |
|---|---|---|---|
| 1 | Los 3 endpoints sin token | 401 | ✅ 401 en los 3 |
| 2 | Token con emisor válido pero firmado con otra clave | 401 | ✅ 401 |
| 3 | Token real de la Auth API usado en la Credit API | Aceptado | ✅ 200 |
| 4 | Catálogo de tipos | 3 tipos leídos de `creditdb` | ✅ 15.50 / 8.50 / 22.00 |
| 5 | Simular con cada tipo | La tasa aplicada es la del tipo | ✅ en los 3 tipos |
| 6 | Enviar `tasaAnual: 0.01` en el cuerpo | Se ignora; manda el tipo | ✅ aplica 22.00 |
| 7 | Tipo inexistente | 400 con mensaje | ✅ |
| 8 | Monto fuera de rango (50 y 5 000 000) | 400 con mensaje | ✅ |
| 9 | Plazo fuera de rango (0 y 600) | 400 con mensaje | ✅ |
| 10 | Historial del usuario | Sus simulaciones, más reciente primero | ✅ |
| 11 | Usuario nuevo consulta historial | Vacío: no ve las de otros | ✅ `[]` |
| 12 | Simulación de un usuario | No altera el historial de otro | ✅ |
| 13 | Claves foráneas de `creditdb` | Ninguna apunta a `authdb` | ✅ solo `simulaciones → tipos_credito` |

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
| 1 | 16/09/2026 | Registro, login y sesión persistente con JWT verificado contra la base `authdb`. 12 pruebas de aceptación en verde. | Se conservó **PostgreSQL 17**, ya instalado en el equipo, en lugar de instalar la 18: dos servidores en el mismo puerto habrían costado tiempo de configuración sin aportar valor al producto. Ejemplo directo del principio *responder ante el cambio sobre seguir un plan*. |
| 2 | 18/09/2026 | Credit API con regla tipo → tasa, tablas francesa y alemana exactas al centavo, historial por usuario y endpoints protegidos con el JWT de la Auth API. 42 pruebas unitarias y 13 de aceptación en verde. | Se corrigió un defecto de redondeo acumulativo en el método francés, detectado por las propias pruebas del sprint. Se añadió una prueba de regresión y se documentó el caso. Se unificó la versión de EF Core (10.0.12) para eliminar un conflicto de dependencias. |
| 3 | | | |
