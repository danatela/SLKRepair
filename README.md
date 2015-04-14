SLKRepair
=========

Program that repairs broken Excel SYLK (SLK) files.

Problem description
-------------------
SLK files are designed to convert some data generated by old applications to Excel format. Though it has several issues. One of them is crashing Excel when number of used formatting styles exceeds certain limit (nearly 255). Also before crashing occurs, document becomes weird-looking and takes a long time to be opened. Especially this can be remarkable in Excel 2007-2013, because it writes down new stylesheet (records like `P;F` or `P;E`) everytime when you save the document. Just cleaning the document title doesn't help, because every cell refers to special style number. For successful repair, you have to correct those references.

Solution
--------
SlkRepair cleans up document. It searches for only used styles, writes them into document header and corrects references to them. Reparation is in-place and old document is saved with additional `~` sign. Next launches will rewrite backup, so if you don't like result document, backup it manually.

**Additional functions**: file reduces it's size. Empty cells, which do not contain data or borders will become removed. Also, row heights (records starting with `F;M`) are eliminated (my example old program ignores them).

**Issues:**
  * Does not delete neighbor cells near bordered cells.
  * Does not delete cells with space bars and non-printable characters.
  * Does not delete protected and neighbor empty cells. Result contains "columns" of records like <code>C;X1 ... C;X<i>n</i></code>
  * You can't make a cup of coffee with this program.

Usage
-----

    slkrepair <files> [/C[ONVERT]:(0|1|Y[ES]|N[O])].
  
**Example**: `slkrepair report.slk sklrep01.slk rep.slk /c:0`

Letters recognition
-------------------

Program decodes long strings with russian letters and saves them with DOS encoding. You can setup different encodings. Decoding can be enabled with key `/C[ONVERT]:1` and disabled with the same key with parameter 0: `/C[ONVERT]:0`. To set up default converting mode, you should edit slkrepair.exe.config:

    <?xml version="1.0"?>
     <configuration>
    ...
        <userSettings>
            <SlkRepair.Properties.Settings>
                <setting name="Convert" serializeAs="String">
	            <!-- Conversion behavior is set below -->
                    <value>False</value>
                </setting>
            </SlkRepair.Properties.Settings>
        </userSettings>
    </configuration>
    </file>

Materials
---------
Constructions are taken from <a href=http://en.wikipedia.org/wiki/SYLK>Wikipedia article</a>
