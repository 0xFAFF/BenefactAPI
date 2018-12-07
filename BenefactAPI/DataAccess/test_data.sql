
insert into cards (id, title, description) values
(1, 'Get MD Working', 'Some Markdown\n=====\n\n```csharp\n    var herp = \"derp\";\n```'),
(2, '😈😈😈😈😈😈', 'Make sure UTF8 works 😑');

insert into categories (id, name) values
(1, 'Story'),
(2, 'Dev Task'),
(3, 'Business Boiz'),
(4, 'Bug');

insert into category_mapping (cardid, categoryid) values
(1, 1), (1, 2), (1, 3), (1, 4),
(2, 1);