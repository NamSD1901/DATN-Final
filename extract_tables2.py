import zipfile
import xml.etree.ElementTree as ET

def extract_tables(path):
    document = zipfile.ZipFile(path)
    xml_content = document.read('word/document.xml')
    document.close()
    
    tree = ET.XML(xml_content)
    tables = []
    
    for table in tree.iter('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}tbl'):
        table_data = []
        for row in table.iter('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}tr'):
            row_data = []
            for cell in row.iter('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}tc'):
                texts = [node.text
                         for paragraph in cell.iter('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}p')
                         for node in paragraph.iter('{http://schemas.openxmlformats.org/wordprocessingml/2006/main}t')
                         if node.text]
                row_data.append(''.join(texts))
            table_data.append(row_data)
        tables.append(table_data)
        
    return tables

if __name__ == '__main__':
    tables = extract_tables(r'e:\DATN\MyPetClinic\Petlover_Bao cao tot nghiep.docx')
    with open(r'e:\DATN\MyPetClinic\tables_output.txt', 'w', encoding='utf-8') as f:
        for i, tbl in enumerate(tables):
            f.write(f"--- Table {i} ---\n")
            for row in tbl:
                f.write(" | ".join(row) + "\n")
            f.write("\n")
print("Done")
