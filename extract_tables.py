import sys
try:
    from docx import Document
except ImportError:
    import os
    os.system('pip install python-docx')
    from docx import Document

doc = Document(r'e:\DATN\MyPetClinic\Petlover_Bao cao tot nghiep.docx')
with open(r'e:\DATN\MyPetClinic\tables_output.txt', 'w', encoding='utf-8') as f:
    for i, table in enumerate(doc.tables):
        f.write(f"--- Table {i} ---\n")
        for row in table.rows:
            f.write(" | ".join([cell.text.replace('\n', ' ') for cell in row.cells]) + "\n")
        f.write("\n")
print("Tables extracted to tables_output.txt")
