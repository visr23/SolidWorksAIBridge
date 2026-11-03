# Architectural Modeling Standard V1

מסמך זה מתעד את שיטת העבודה המתפתחת לבנייה אוטומטית של מיניאטורות אדריכליות מתמונות בתוך SolidWorks. הוא מגדיר design rules, feature logic ו־manufacturing logic למערכת העתידית, על בסיס למידה ממידול ידני.

זהו תקן תכנון מנחה, ולא תיאור של יכולות שכבר מומשו. בפרט, ההמלצות ל־Pattern ול־Mirror אינן הוראה לשנות כעת את מנגנון השורות והקומות הקיים, המבוסס על סקיצות משותפות. מימוש עתידי ייעשה בנפרד, תוך שמירת ההתנהגות העובדת ובדיקת התאמת הכללים למקרה.

## 1. Core Pipeline

המערכת העתידית לא צריכה לעבוד ישירות:

```text
Photo → SolidWorks
```

אלא:

```text
Photo → Architectural Analysis → Simplified Construction Plan → Parametric SolidWorks Model
```

שלב ה־Construction Plan צריך להכיל בעיקר:

- building outer envelope
- floor levels
- primary bays
- primary openings
- major recesses/projections
- symmetry axes
- depth zones
- manufacturing body separation

פרטים קטנים יתווספו בשלבים מאוחרים יותר.

## 2. Blockout First

תהליך הבנייה מתחיל ממסה אדריכלית פשוטה. סדר הבנייה המועדף:

1. Main facade / wall bodies
2. Primary openings
3. Major recesses
4. Major projections
5. Secondary architectural elements
6. Repeated features
7. Details
8. Manufacturing-specific parts

אין להתחיל מפרטים קטנים. יש להתחיל בגישה שמרנית: לבנות קודם את המסה הראשית, ולאחר מכן להסיר או להוסיף geometry באמצעות features נוספים.

## 3. Blueprint / Construction Reference

כאשר נוצרת תוכנית דו־ממדית מתמונה לצורך modeling reference, יש להשתמש ב:

- black lines
- white background
- minimal internal detail
- no unnecessary shutter/slat/window-detail lines during wall blockout
- focus on outer contour, openings, recesses and structural divisions

ה־Blueprint הראשוני מיועד לבניית הקירות, ולא לייצוג סופי של כל הפרטים.

## 4. Architectural Scene Structure

יש לפרק את הבניין להיררכיה אדריכלית. לדוגמה:

```text
Building
├── FrontFacade
│   ├── LeftBay
│   ├── CenterBay
│   └── RightBay
├── SideFacade
├── RearFacade
├── Entrance
└── Roof
```

בתוך כל Bay:

```text
Bay
├── Windows
├── Recesses
├── Railings
├── Shutters
└── OtherFeatures
```

אין להתייחס לכל feature כאובייקט בלתי תלוי כאשר קיימת ביניהם מערכת יחסים אדריכלית.

## 5. Repetition and Pattern Logic

כאשר geometry חוזרת:

- create one master feature
- prefer Linear Pattern
- prefer Mirror
- avoid independently modeling every instance

לדוגמה:

```text
MasterWindow
→ VerticalLinearPattern
→ MirrorAcrossFacade
```

או:

```text
MasterRecess
→ VerticalPattern
→ Mirror
```

אם Feature B שייך ל־Feature A, ו־A מופיע ב־Pattern, יש לבדוק האם B צריך לרשת את אותו Pattern. Pattern propagation הוא כלל חשוב: יש לשמר את הקשר בין הרכיב הראשי לרכיבים השייכים אליו בעת יצירת החזרות.

## 6. Symmetry

כאשר הצדדים סימטריים:

- build one master side
- mirror it

אם ציר הסימטריה תואם ל־Front/Right/Top Plane קיים, ניתן להשתמש בו. אחרת:

- create a dedicated Reference Plane
- place it on the true architectural symmetry axis
- mirror using that plane

אין להניח ש־Right Plane תמיד מייצג את ציר הסימטריה.

## 7. Shared Datums Across Elevations

Front, Side ו־Rear elevations אינם מודלים נפרדים. יש להשתמש ב־shared architectural datums:

- floor heights
- window sill heights
- window head heights
- roof line
- projection lines
- horizontal structural bands

כאשר Datum כבר ידוע מחזית אחת, אין להעריך אותו מחדש מתמונה אחרת ללא צורך.

```text
Front facade floor datum
→ reused by side windows
→ reused by rear windows
```

## 8. Unique vs Repeated Openings

יש לסווג פתחים ל:

```text
Unique
RepeatedVertical
RepeatedHorizontal
RepeatedGrid
```

Unique openings:

- separate cut/feature

Repeated openings:

- one master opening
- pattern instances

אין לכפות Pattern כאשר אין ראיה אמיתית לחזרתיות.

## 9. Wall First, Openings Second

במיוחד בחזיתות צד ואחוריות, תהליך העבודה המועדף הוא:

```text
Wall Body
→ Cut-Extrude Master Opening
→ Pattern
→ Unique Cuts
→ Sills / Frames / Details
```

גישה זו עדיפה בדרך כלל על Sketch גדול שמכיל מראש את כל החלונות. זהו עיקרון לתכנון עתידי, ולא דרישה להחליף כעת את מימוש הסקיצות המשותפות הקיים.

## 10. Depth Hierarchy

לחזית יש מספר depth zones. לדוגמה, ביחידות המיניאטורה:

```text
MainFacadePlane = 0
MinorProjection = +0.8mm
MediumProjection = ±1.2mm
DeepRecess = inferred separately
```

העומק המדויק בעולם האמיתי אינו תמיד זמין. במקרה כזה משתמשים ב־miniature representation depth, ונמנעים מ־false precision.

יש להבדיל בין:

- measured depth
- inferred depth
- stylized / printable depth

## 11. Global Variables / Standard Parameters

הסטנדרט הנוכחי כולל את ערכי ברירת המחדל הבאים למיניאטורה:

```text
"WallThickness" = 3mm
"InternalWallThickness" = 2mm
"MinorProjection" = 0.8mm
"MediumProjection" = 1.2mm
"AssemblyClearance" = 0.2mm
```

יש להשתמש ב־Global Variables במקום להקליד מספרים חוזרים ידנית. שמות משתנים צריכים לתאר תפקיד, ולא Feature ספציפי.

Good:

```text
WallThickness
MinorProjection
AssemblyClearance
```

Avoid:

```text
BossExtrude7Depth
WindowBoss3
```

## 12. Printable Thickness vs Full Depth

לא כל קיר צריך למלא את מלוא המרחק עד surface אחרת. כאשר המרחק עמוק, לדוגמה 7–8mm, אך אין צורך בקיר מלא לכל העומק:

- use controlled wall thickness
- default structural miniature wall thickness ≈ `WallThickness`
- internal walls may use `InternalWallThickness`

אין ליצור מסה מלאה ללא צורך.

```text
Needs full depth?
YES → Up To Surface
NO  → controlled printable thickness
```

המטרה היא לחסוך חומר ומשקל.

## 13. Prefer Geometry-Driven Termination

כאשר Feature צריך להגיע לגיאומטריה קיימת, יש להעדיף:

- Up To Surface
- Up To Body
- Convert Entities
- Coincident references

יש להימנע ממידות Blind שרירותיות כאשר geometry קיימת יכולה להגדיר את הגבול.

```text
EntranceWall
→ Up To Surface
→ target = existing internal wall
```

## 14. Convert Entities Usage

Convert Entities מומלץ כאשר קיימת תלות אמיתית בגיאומטריה קיימת. לדוגמה:

- closure wall follows existing body contour
- roof follows building perimeter
- side wall continues existing entrance step
- feature must align exactly to an existing edge

אין להשתמש ב־Convert Entities באופן מופרז רק כדי להימנע מציור geometry. ההפניות צריכות להישאר מובנות ויציבות.

## 15. Thin Feature Rules

כאשר משתמשים ב־Thin Feature על open sketch lines:

- draw lines consistently in the same direction
- keep thickness direction consistent
- use shared thickness variables
- compensate sketch position when required so final wall aligns correctly

יש להתייחס לכיוון הקו ב־Thin Feature כנתון בעל משמעות גיאומטרית.

## 16. Body Separation by Manufacturing Logic

אין למזג Bodies אוטומטית רק מפני שהם נוגעים זה בזה. מבנה הגופים נקבע לפי:

- print orientation
- assembly
- painting
- support requirements
- depth differences
- material saving
- ease of handling

עיקרון מנחה:

`Merge according to manufacturing unit, not geometric contact.`

## 17. Separate Depth Zones

אם שני נפחי שקע נראים קשורים במבט חזיתי, אך יש להם עומקים שונים:

- separate feature chains
- independent depth definitions
- separate bodies when manufacturing requires it

```text
CentralRecessBody
- shallow depth
- printed separately

BalconyRecessBody
- deeper depth
- printed separately
```

אין לפצות על עומק משותף שגוי באמצעות הוספת חומר מלא ומיותר בהמשך.

## 18. AI Depth Revision

ניתוח התמונה הראשוני עלול להיות שגוי. לדוגמה:

```text
initial assumption:
same_depth = true
```

אם ראיות חזותיות מאוחרות יותר מראות:

- different side wall length
- different ceiling depth
- different shadow
- different back wall position

יש לעדכן את ההנחה:

```text
same_depth = false
→ split depth zones
→ split feature chains
→ split bodies if necessary
```

המערכת העתידית חייבת לתמוך בתיקון הנחות גיאומטריות מוקדמות.

## 19. Manufacturing Classes

כל אובייקט מזוהה צריך לתמוך בעתיד בסיווג:

```text
manufacturing_class:
- main_print
- separate_print
- post_process_part
- ignore
```

Main print:

- primary walls
- structural recesses

Separate print:

- roof panels
- overhangs
- some railings

Post-process:

- cables
- AC units
- planters
- signs
- pipes

Ignore:

- dirt
- stains
- vegetation
- temporary signs where irrelevant

## 20. Avoid Support Through Body Separation

אם רכיב בולט מחייב תמיכות מיותרות על גוף גדול בהרבה:

- separate it
- print it in a better orientation
- glue/assemble afterward

לדוגמה, Entrance roof / canopy:

- separate body
- not merged to facade
- support avoidance is the reason

הצורך בכיוון הדפסה מתאים יכול אפוא להוביל להפרדת גופים.

## 21. Assembly Clearance

כאשר חלק צריך להיכנס למכלול העשוי ממספר קירות מודפסים, יש להחיל מרווח הרכבה במקום geometry ללא מרווח:

```text
AssemblyClearance = 0.2mm
```

שימושים טיפוסיים:

- roof inserted between four walls
- internal closure panels
- glued assembly components

דוגמת פרופיל גג:

```text
Existing building perimeter
→ offset by AssemblyClearance
→ extrude RoofThickness
```

כיוון ה־offset צריך ליצור מרווח בפועל: בחלק הנכנס בין קירות, יש להקטין את הפרופיל בהתאם. `RoofThickness` יוגדר לפי תכנון החלק; אין במסמך זה ערך ברירת מחדל עבורו.

## 22. Wall Thickness Compensation

כאשר ממקמים פתח ליד קיר אחר, יש לכלול את עובי הקיר הסמוך בלוגיקת המיקום. אין למדוד תמיד מהמעטפת החיצונית.

```text
WindowOffset =
ArchitecturalSpacing
+ WallThickness
```

כאשר ניתן, עדיף להפנות ישירות לפאת הקיר הנכונה.

## 23. Entrance Modeling

כניסה אינה פתח יחיד. יש לייצג אותה כמכלול:

```text
Entrance
├── Landing
├── Stairs
├── SideWalls
├── InternalWalls
├── Ceiling / Canopy
└── Doorway / Passage
```

כל אחד מהרכיבים עשוי להיות גוף נפרד לצורכי ייצור.

## 24. Stair Logic

כאשר המדרגות אחידות:

```text
MasterStep
→ LinearPattern
```

פרמטרי המדרגות:

```text
step_count
rise
tread
width
```

אין למדל כל מדרגה באופן עצמאי.

## 25. Angled / Non-Orthogonal Geometry

אין לפשט אוטומטית אדריכלות זוויתית נראית לעין ל־90°. כאשר קיר נראה מתחדד או נטוי:

- estimate clean angle
- keep it parametric
- use a simple value that reproduces visual appearance

```text
EntranceWallAngle ≈ 4deg
```

זהו ערך דוגמה בלבד, ולא זווית ברירת מחדל לכל כניסה.

## 26. Hidden Geometry Inference

כאשר צמחייה או אובייקט אחר מסתירים geometry, יש להיעזר בלוגיקה אדריכלית חוזרת.

לדוגמה, אם:

- same window pair repeats across multiple floors
- upper floors clearly show continuous sill
- lower floor is obscured

אז:

- infer same sill on lower floor
- mark it as inferred
- do not treat it as directly observed

כל feature מוסק צריך לתמוך בעתיד במידע כגון:

```text
source = directly_observed | inferred_from_pattern | estimated
confidence = 0..1
```

## 27. Side / Party Wall Context

לא כל קיר הוא חזית. סוגי קירות אפשריים:

```text
external_facade
rear_facade
side_facade
party_wall
connection_wall
```

כאשר הבניין צמוד לבניין אחר:

- connected wall may remain plain
- do not invent openings
- use the architectural context, not only pixel appearance

## 28. Seam Placement

כאשר יוצרים קירות להדפסה בנפרד, יש להעדיף מיקומי תפר שיהיו פחות בולטים לאחר ההרכבה.

לדוגמה: לשחל קיר צד כלפי פנים, כך שתפר החיבור יופיע בצד ולא בחזית הקדמית.

מיקום התפר הוא החלטת ייצור.

## 29. Feature Scope in Multi-Body Parts

ב־multi-body parts:

- use Selected Bodies when appropriate
- avoid accidental merges
- explicitly define target body
- do not allow a Boss/Cut to affect unrelated bodies

ייצוג רעיוני:

```text
target_body = SideWall
merge_with_target = true
affect_other_bodies = false
```

## 30. Feature Tree Philosophy

יש להעדיף:

- simple features
- understandable dependencies
- patterns
- mirrors
- surface-driven termination
- global variables

יש להימנע מ:

- giant sketches containing every object
- repeated manual dimensions
- patch features that compensate for earlier wrong geometry
- unnecessary solid volume
- early merging of bodies

עיקרון כללי:

`Simple features + strong relationships > complex monolithic features`

## 31. Future AI Design Intent

המערכת העתידית אינה צריכה רק לזהות אובייקטים. עליה לזהות יחסים:

```text
Window
↓
Recess belongs to Window
↓
Sill belongs to WindowGroup
↓
WindowGroup repeats vertically
↓
Entire group may mirror horizontally
```

זהו Design Intent Recognition. על ה־AI למדל אדריכלות כמשפחות features בעלות קשרים, ולא כאוסף צורות מנותקות.

## 32. Current Goal

מסמך V1 זה הוא תקן למידה הנגזר ממידול ידני ב־SolidWorks. אין להתייחס לכל הערכים שבו כמידות אדריכליות אוניברסליות.

ערכים כגון:

```text
0.8mm
1.2mm
2mm
3mm
0.2mm
```

הם מוסכמות ייצור למיניאטורות, והם עשויים להתפתח.

המטרה היא ללמד את המערכת העתידית:

- how to reason
- how to structure geometry
- how to decide what becomes a body
- how to reuse references
- how to reduce material
- how to avoid supports
- how to preserve editability

בשלב זה המסמך מתעד כוונות וכללים בלבד. אין לשנות בעקבותיו קוד ייצור, לממש זיהוי תמונה או לבצע refactor של SolidWorks bridge.
