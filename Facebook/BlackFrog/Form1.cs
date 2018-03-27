using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;

namespace JH.Applications
{
    public partial class Form1 : Form
    {
        class ItemPair
        {
            public ItemPair(string text, string description, string id)
            {
                this.text = text;
                this.description = description;
                this.id = id;
            }
            public string text;
            public string description;
            public string id;
        }

        static int CompareItemPair(ItemPair x, ItemPair y)
        {
            string x1 = "";
            string y1 = "";

            int pos = 0;
            for (int i = 0; i < x.text.Length; i++)
                if (x.text[i] != ' ')
                {
                    pos = i;
                    break;
                }
            for (int i = pos; i < x.text.Length; i++)
                x1 += x.text[i];

            pos = 0;
            for (int i = 0; i < y.text.Length; i++)
                if (y.text[i] != ' ')
                {
                    pos = i;
                    break;
                }
            for (int i = pos; i < y.text.Length; i++)
                y1 += y.text[i];

            return x1.CompareTo(y1);
        }

        private readonly IFacebookClient facebookClient = new FacebookClient();
        string accessToken;
        List<ItemPair> groupList = new List<ItemPair>();
        List<ItemPair> albumList = new List<ItemPair>();
        List<ItemPair> albumSubList = new List<ItemPair>();
        List<ItemPair> albumSubListSorted = new List<ItemPair>();
        List<ItemPair> alList = new List<ItemPair>();
        int selectedGroup;
        int selectedAlbum;
        string filenameXml;
        string filenameHtml;
        XmlDocument xmlBase;

        public Form1()
        {
            InitializeComponent();

            saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "html";

            button3.BackColor = Color.Green;

            Text = "BlackFrog" + " ver. 1.20"; //remember update of XSLT
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ClearAll();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            accessToken = textBox1.Text;

            ClearAll();
            DisableAll();

            Thread thread = new Thread(new ThreadStart(GoGroups));
            thread.Start();

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedGroup = comboBox1.SelectedIndex;

            ClearAlbums();
            DisableAll();

            Thread thread = new Thread(new ThreadStart(GoAlbums));
            thread.Start();

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedAlbum == -1)
            {
                selectedAlbum = comboBox2.SelectedIndex;
                return;
            }

            selectedAlbum = comboBox2.SelectedIndex;

            DialogResult result = saveFileDialog.ShowDialog();
            {
                if (result == DialogResult.OK)
                {
                    filenameHtml = saveFileDialog.FileName;
                    filenameXml = Path.GetDirectoryName(filenameHtml) + "\\" + Path.GetFileNameWithoutExtension(filenameHtml) + ".xml";
                }
                else
                    return;
            }

            DisableAll();

            Thread thread = new Thread(new ThreadStart(GoSingle));
            thread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedGroup == -1 || selectedAlbum == -1)
                return;

            DialogResult result = saveFileDialog.ShowDialog();
            {
                if (result == DialogResult.OK)
                {
                    filenameHtml = saveFileDialog.FileName;
                    filenameXml = Path.GetDirectoryName(filenameHtml) + "\\" + Path.GetFileNameWithoutExtension(filenameHtml) + ".xml";
                }
                else
                    return;
            }

            DisableAll();

            Thread thread = new Thread(new ThreadStart(GoAll));
            thread.Start();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (selectedGroup == -1)
                return;

            DialogResult result = saveFileDialog.ShowDialog();
            {
                if (result == DialogResult.OK)
                {
                    filenameHtml = saveFileDialog.FileName;
                    filenameXml = Path.GetDirectoryName(filenameHtml) + "\\" + Path.GetFileNameWithoutExtension(filenameHtml) + ".xml";
                }
                else
                    return;
            }

            DisableAll();

            Thread thread = new Thread(new ThreadStart(GoAllFeed));
            thread.Start();

        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue != 13)
                return;

            DisableAll();
            albumSubList.Clear();

            foreach (ItemPair album in albumList)
            {
                if ((album.text.Length >= textBox2.Text.Length && album.text.Substring(0, textBox2.Text.Length).ToLower() == textBox2.Text.ToLower()) || textBox2.Text == "")
                {
                    albumSubList.Add(album);
                }

            }

            Invoke(new Action(() =>
            {
                comboBox2.Items.Clear();
                comboBox2.Text = "";

                albumSubListSorted.Clear();
                foreach (ItemPair album in albumSubList)
                    albumSubListSorted.Add(album);
                albumSubListSorted.Sort(CompareItemPair);

                foreach (ItemPair album in albumSubListSorted)
                    comboBox2.Items.Add(album.text);

                EnableAll();
                selectedAlbum = -1;
                if (albumSubList.Count > 0)
                    comboBox2.SelectedIndex = 0;

            }));
        }

        void GoAllFeed()
        {
            string groupText = groupList[selectedGroup].text;
            string groupDescription = groupList[selectedGroup].description;
            string groupId = groupList[selectedGroup].id;

            UpdateStatus("Processing all feeds, it may take several minutes" + "\r\n");

            InitDoc(groupText, groupDescription, groupId);

            XmlDocument doc = GetAsyncAllII("photos", groupId + "/feed", "fields=link,message,story,object_id,id,created_time");

            XmlNode imported = xmlBase.ImportNode(doc.DocumentElement, true);
            xmlBase.FirstChild.LastChild.InsertAfter(imported, null);

            XmlNodeList l = GetNode(xmlBase.FirstChild.ChildNodes, "albums").FirstChild.ChildNodes;
            int count0 = 1;
            foreach (XmlNode node in l)
            {
                UpdateStatus("Processing outer loop node " + count0.ToString() + " of " + l.Count.ToString() + "\r\n");
                count0++;
                if (node.Name == "page")
                {
                    XmlNodeList l1 = node.ChildNodes;
                    int count1 = 1;
                    foreach (XmlNode node1 in l1)
                    {
                        UpdateStatus("Processing inner loop node " + count1.ToString() + " of " + l1.Count.ToString() + "\r\n");
                        count1++;

                        if (node1.Name == "data")
                        {
                            XmlNode node2 = GetNode(node1.ChildNodes, "object_id");

                            if (node2 != null)
                            {
                                XmlDocument doc0 = GetAsyncAll("tags", node2.InnerText, "fields=tags");
                                if (doc0 != null)
                                {
                                    XmlNode node5 = xmlBase.ImportNode(doc0.DocumentElement, true);
                                    node1.AppendChild(node5);
                                }
                                XmlDocument doc1 = GetComments(node2.InnerText);
                                if (doc1 != null)
                                {
                                    XmlNode node3 = xmlBase.ImportNode(doc1.DocumentElement, true);
                                    node1.AppendChild(node3);
                                }
                                XmlDocument doc2 = GetLikes(node2.InnerText);
                                if (doc2 != null)
                                {
                                    XmlNode node4 = xmlBase.ImportNode(doc2.DocumentElement, true);
                                    node1.AppendChild(node4);
                                }
                            }
                        }
                    }
                }
            }
            XmlNode version = xmlBase.CreateNode(XmlNodeType.Element, "version", "");
            version.InnerText = "This document was generated using " + Text + ", " + DateTime.Now.ToString();
            xmlBase.FirstChild.AppendChild(version);
            xmlBase.Save(filenameXml);

            TransformXml2Html("feeds.xslt");

            EnableAll();

            UpdateStatus("Processing feeds done\r\n");
        }

        void GoGroups()
        {
            try
            {
                UpdateStatus("Getting group list\r\n");

                XmlDocument xmlDocument = GetAsyncAll("root", "me/groups", "fields=name,id,description");

                XmlNode data = xmlDocument.FirstChild.FirstChild.FirstChild;

                if (data == null)
                {
                    EnableAll();
                    return;
                }

                XmlNodeList list = xmlDocument.FirstChild.ChildNodes;

                foreach (XmlNode nod in list)
                {
                    XmlNodeList lst = nod.ChildNodes;
                    foreach (XmlNode node in lst)
                    {
                        if (node.Name == "data")
                        {
                            XmlNodeList l = node.ChildNodes;
                            XmlNode nod1 = GetNode(l, "description");
                            string description = "";
                            if (nod1 != null)
                                description = nod1.InnerText;
                            ItemPair pair = new ItemPair(GetNode(l, "name").InnerText, description, GetNode(l, "id").InnerText);
                            groupList.Add(pair);
                        }
                    }
                }

                Invoke(new Action(() =>
                {
                    foreach (ItemPair group in groupList)
                        comboBox1.Items.Add(group.text);

                    EnableAll();

                    comboBox1.SelectedIndex = 0;

                    UpdateStatus("Getting group list done\r\n");
                }));
            }
            catch
            {
                EnableAll();
                ClearAll();

                MessageBox.Show("Probably missing or expired access token");
            }
        }

        void GoAlbums()
        {
            string group = groupList[selectedGroup].text;
            string groupId = groupList[selectedGroup].id;

            UpdateStatus("Getting album list\r\n");

            XmlDocument xmlDocument = GetAsyncAll("root", groupId + "/albums", "");

            XmlNode data = xmlDocument.FirstChild.FirstChild.FirstChild;

            if (data == null)
            {
                UpdateStatus("Getting album list done\r\n");
                EnableAll();
                return;
            }

            XmlNodeList list = xmlDocument.FirstChild.ChildNodes;

            foreach (XmlNode nod in list)
            {
                XmlNodeList lst = nod.ChildNodes;
                foreach (XmlNode node in lst)
                {
                    if (node.Name == "data")
                    {
                        XmlNodeList l = node.ChildNodes;
                        ItemPair pair = new ItemPair(GetNode(l, "name").InnerText, "", GetNode(l, "id").InnerText);
                        albumList.Add(pair);

                        if (pair.text.Substring(0, textBox2.Text.Length).ToLower() == textBox2.Text.ToLower() || textBox2.Text == "")
                        {
                            albumSubList.Add(pair);
                        }
                    }
                }
            }

            Invoke(new Action(() =>
            {
                albumSubListSorted.Clear();
                foreach (ItemPair album in albumSubList)
                    albumSubListSorted.Add(album);
                albumSubListSorted.Sort(CompareItemPair);

                foreach (ItemPair album in albumSubListSorted)
                    comboBox2.Items.Add(album.text);

                EnableAll();

                comboBox2.SelectedIndex = 0;

                UpdateStatus("Getting album list done\r\n");
            }));
        }

        void GoSingle()
        {
            string groupText = groupList[selectedGroup].text;
            string groupDescription = groupList[selectedGroup].description;
            string groupId = groupList[selectedGroup].id;
            string albumText = albumSubListSorted[selectedAlbum].text;
            string albumId = albumSubListSorted[selectedAlbum].id;

            UpdateStatus("Processing album " + (selectedAlbum + 1).ToString() + " of " + 1.ToString() + "\r\n");

            InitDoc(groupText, groupDescription, groupId);

            XmlDocument doc2 = CreateAlbum(albumText, albumId);
            XmlNode imported;
            XmlDocument doc3 = GetComments(albumId);
            if (doc3 != null)
            {
                imported = doc2.ImportNode(doc3.DocumentElement, true);
                doc2.DocumentElement.AppendChild(imported);
            }
            XmlDocument doc4 = GetPhotos(albumId, 1);
            if (doc4 != null)
            {
                imported = doc2.ImportNode(doc4.DocumentElement, true);
                doc2.DocumentElement.AppendChild(imported);
            }
            XmlDocument doc5 = GetLikes(albumId);
            if (doc5 != null)
            {
                imported = doc2.ImportNode(doc5.DocumentElement, true);
                doc2.DocumentElement.AppendChild(imported);
            }
            imported = xmlBase.ImportNode(doc2.DocumentElement, true);
            xmlBase.FirstChild.LastChild.InsertAfter(imported, null);

            XmlNode version = xmlBase.CreateNode(XmlNodeType.Element, "version", "");
            version.InnerText = "This document was generated using " + Text + ", " + DateTime.Now.ToString();
            xmlBase.FirstChild.AppendChild(version);
            xmlBase.Save(filenameXml);

            TransformXml2Html("single.xslt");

            EnableAll();

            UpdateStatus("Processing album done\r\n");
        }

        void GoAll()
        {
            string groupText = groupList[selectedGroup].text;
            string groupDescription = groupList[selectedGroup].description;
            string groupId = groupList[selectedGroup].id;


            InitDoc(groupText, groupDescription, groupId);

            XmlNode n2 = null;
            int albumIndex = 0;
            foreach (ItemPair album in albumSubList)
            {
                UpdateStatus("Processing album " + (albumIndex + 1).ToString() + " of " + albumSubList.Count.ToString() + "\r\n");

                string albumText = albumSubList[albumIndex].text;
                string albumId = albumSubList[albumIndex].id;

                XmlDocument doc2 = CreateAlbum(albumText, albumId);
                XmlNode imported;
                XmlDocument doc3 = GetComments(albumId);
                if (doc3 != null)
                {
                    imported = doc2.ImportNode(doc3.DocumentElement, true);
                    doc2.DocumentElement.AppendChild(imported);
                }
                XmlDocument doc4 = GetPhotos(albumId, 1);
                if (doc4 != null)
                {
                    imported = doc2.ImportNode(doc4.DocumentElement, true);
                    doc2.DocumentElement.AppendChild(imported);
                }
                XmlDocument doc5 = GetLikes(albumId);
                if (doc5 != null)
                {
                    imported = doc2.ImportNode(doc5.DocumentElement, true);
                    doc2.DocumentElement.AppendChild(imported);
                }
                imported = xmlBase.ImportNode(doc2.DocumentElement, true);
                xmlBase.FirstChild.LastChild.InsertAfter(imported, n2);
                n2 = imported;
                albumIndex++;
            }

            XmlNode version = xmlBase.CreateNode(XmlNodeType.Element, "version", "");
            version.InnerText = "This document was generated using " + Text + ", " + DateTime.Now.ToString();
            xmlBase.FirstChild.AppendChild(version);
            xmlBase.Save(filenameXml);

            xmlBase.Save(filenameXml);

            TransformXml2Html("all.xslt");

            EnableAll();

            UpdateStatus("Processing albums done\r\n");
        }

        void InitDoc(string groupText, string groupDescription, string groupId)
        {
            xmlBase = new XmlDocument();
            XmlNode e = xmlBase.CreateNode(XmlNodeType.Element, "root", "");
            xmlBase.AppendChild(e);

            XmlNode gName = xmlBase.CreateNode(XmlNodeType.Element, "groupName", "");
            gName.InnerText = groupText;
            e.AppendChild(gName);

            XmlNode gDescription = xmlBase.CreateNode(XmlNodeType.Element, "groupDescription", "");
            gDescription.InnerText = groupDescription;
            e.AppendChild(gDescription);

            XmlNode gId = xmlBase.CreateNode(XmlNodeType.Element, "groupId", "");
            gId.InnerText = groupId;
            e.AppendChild(gId);

            XmlNode nAlbums = xmlBase.CreateNode(XmlNodeType.Element, "albums", "");
            e.AppendChild(nAlbums);
        }

        XmlDocument CreateAlbum(string albumText, string albumId)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlNode nAlbum = xmlDoc.CreateNode(XmlNodeType.Element, "album", "");
            xmlDoc.AppendChild(nAlbum);

            XmlNode aName = xmlDoc.CreateNode(XmlNodeType.Element, "albumName", "");
            aName.InnerText = albumText;
            xmlDoc.DocumentElement.InsertAfter(aName, null);

            XmlNode aId = xmlDoc.CreateNode(XmlNodeType.Element, "albumId", "");
            aId.InnerText = albumId;
            xmlDoc.DocumentElement.InsertAfter(aId, aName);

            XmlDocument xmlDocument = GetAsyncAll("root", albumId, "fields=description");
            XmlNode ndescription = GetNode(xmlDocument.FirstChild.FirstChild.ChildNodes, "description");
            if (ndescription != null)
            {
                XmlNode imported = xmlDoc.ImportNode(ndescription, true);
                xmlDoc.DocumentElement.InsertAfter(imported, aId);
            }

            return xmlDoc;
        }

        XmlDocument GetPhotos(string albumId, int nbr)
        {
            XmlDocument xmlDocument = GetAsyncAll("photos", albumId + "/photos", "fields=source,name,tags,created_time&type=uploaded");

            XmlNode data = xmlDocument.FirstChild.FirstChild.FirstChild;

            if (data == null)
            {
                return null;
            }

            XmlNodeList list = xmlDocument.FirstChild.ChildNodes;

            foreach (XmlNode nod in list)
            {
                XmlNodeList lst = nod.ChildNodes;
                foreach (XmlNode node in lst)
                {
                    {
                        if (node.Name == "data")
                        {
                            XmlNode photoId = GetNode(node.ChildNodes, "id");

                            XmlDocument l = GetComments(photoId.InnerText);
                            XmlDocument l1 = GetLikes(photoId.InnerText);
                            if (l != null)
                            {
                                XmlNode importedDocument = xmlDocument.ImportNode(l.DocumentElement, true);
                                node.AppendChild(importedDocument);
                            }
                            if (l1 != null)
                            {
                                XmlNode importedDocument = xmlDocument.ImportNode(l1.DocumentElement, true);
                                node.AppendChild(importedDocument);
                            }
                        }
                    }
                }
            }
            return xmlDocument;
        }

        private XmlDocument GetComments(string nodeId)
        {
            XmlDocument xmlDocument = GetAsyncAll("comments", nodeId + "/comments", "fields=message,from,attachment,created_time");

            XmlDocument doc = null;
            XmlNode n2 = null;
            XmlNodeList list = xmlDocument.FirstChild.ChildNodes;
            foreach (XmlNode nod in list)
            {
                XmlNodeList lst = nod.ChildNodes;
                foreach (XmlNode node in lst)
                {
                    if (node.Name == "data")
                    {
                        if (doc == null)
                        {
                            doc = new XmlDocument();
                            XmlNode e = doc.CreateNode(XmlNodeType.Element, "comments", "");
                            doc.AppendChild(e);
                            n2 = null;
                        }
                        XmlNode n = doc.CreateNode(XmlNodeType.Element, "comment", "");
                        doc.DocumentElement.InsertAfter(n, n2);
                        n2 = n;
                        XmlNode n1 = doc.CreateNode(XmlNodeType.Element, "text", "");
                        n1.InnerText = GetNode(node.ChildNodes, "from").FirstChild.InnerText + ": " + GetNode(node.ChildNodes, "message").InnerText;
                        n.InsertAfter(n1, null);
                        XmlNode n6 = n1;
                        XmlNode n7 = doc.CreateNode(XmlNodeType.Element, "created_time", "");
                        n7.InnerText = GetNode(node.ChildNodes, "created_time").InnerText;
                        n.InsertAfter(n7, null);
                        XmlNode n8 = n7;
                        XmlNode n3 = GetNode(node.ChildNodes, "attachment");
                        if (n3 != null)
                        {
                            XmlNode n4 = GetNode(n3.ChildNodes, "url");
                            if (n4 != null)
                            {
                                XmlNode n5 = doc.CreateNode(XmlNodeType.Element, "attachment", "");
                                n5.InnerText = n4.InnerText;
                                n.InsertAfter(n5, n1);
                                n8 = n5;
                            }
                        }
                        XmlDocument d1 = GetLikes(GetNode(node.ChildNodes, "id").InnerText);
                        if (d1 != null)
                        {
                            XmlNode importNode = doc.ImportNode(d1.DocumentElement, true);
                            n.InsertAfter(importNode, n8);
                        }
                        XmlDocument doc1 = GetReply(GetNode(node.ChildNodes, "id").InnerText);
                        if (doc1 != null)
                        {
                            XmlNode s = doc.ImportNode(doc1.DocumentElement, true);
                            n.InsertAfter(s, n8);
                        }
                    }
                }
            }

            return doc;
        }

        private XmlDocument GetLikes(string nodeId)
        {
            XmlDocument xmlDocument = GetAsyncAll("comments", nodeId + "/likes", "");

            XmlDocument doc = null;
            XmlNode n2 = null;
            XmlNodeList list = xmlDocument.FirstChild.ChildNodes;
            foreach (XmlNode nod in list)
            {
                XmlNodeList lst = nod.ChildNodes;
                foreach (XmlNode node in lst)
                {
                    if (node.Name == "data")
                    {
                        if (doc == null)
                        {
                            doc = new XmlDocument();
                            XmlNode e = doc.CreateNode(XmlNodeType.Element, "likes", "");
                            doc.AppendChild(e);
                            n2 = null;
                        }
                        XmlNode n = doc.CreateNode(XmlNodeType.Element, "name", "");
                        n.InnerText = GetNode(node.ChildNodes, "name").InnerText;
                        doc.DocumentElement.InsertAfter(n, n2);
                    }
                }
            }

            return doc;
        }

        private XmlDocument GetReply(string nodeId)
        {
            XmlDocument xmlDocument = GetAsyncAll("root", nodeId + "/comments", "fields=message,from,created_time");

            XmlDocument doc = null;
            XmlNode n2 = null;
            XmlNodeList list = xmlDocument.FirstChild.ChildNodes;
            foreach (XmlNode nod in list)
            {
                XmlNodeList lst = nod.ChildNodes;
                foreach (XmlNode node in lst)
                {
                    if (node.Name == "data")
                    {
                        if (doc == null)
                        {
                            doc = new XmlDocument();
                            XmlNode e = doc.CreateNode(XmlNodeType.Element, "replies", "");
                            doc.AppendChild(e);
                            n2 = null;
                        }
                        XmlNode n = doc.CreateNode(XmlNodeType.Element, "reply", "");
                        doc.DocumentElement.InsertAfter(n, n2);
                        n2 = n;
                        XmlNode n1 = doc.CreateNode(XmlNodeType.Element, "text", "");
                        n1.InnerText = GetNode(node.ChildNodes, "from").FirstChild.InnerText + ": " + GetNode(node.ChildNodes, "message").InnerText;
                        n.InsertAfter(n1, null);

                        XmlNode n7 = doc.CreateNode(XmlNodeType.Element, "created_time", "");
                        n7.InnerText = GetNode(node.ChildNodes, "created_time").InnerText;
                        n.InsertAfter(n7, n1);

                        XmlDocument d1 = GetLikes(GetNode(node.ChildNodes, "id").InnerText);
                        if (d1 != null)
                        {
                            XmlNode importNode = doc.ImportNode(d1.DocumentElement, true);
                            n.InsertAfter(importNode, n7);
                        }

                        XmlDocument doc1 = GetComments(GetNode(node.ChildNodes, "id").InnerText);
                        if (doc1 != null)
                        {
                            XmlNode s = doc.ImportNode(doc1.DocumentElement, true);
                            n.InsertAfter(s, n7);
                        }
                    }
                }
            }

            return doc;
        }

        public void TransformXml2Html(string xsl)
        {
            XslCompiledTransform transform = new XslCompiledTransform();
            StreamReader stream = new StreamReader(xsl);
            transform.Load(XmlReader.Create(stream));
            transform.Transform(filenameXml, filenameHtml);
            stream.Close();
        }

        XmlNode GetNode(XmlNodeList lst, string name)
        {
            XmlNode nod = null;

            foreach (XmlNode node in lst)
            {
                if (node.Name == name)
                {
                    nod = node;
                    break;
                }
            }

            return nod;
        }

        string CleanXml(string input)
        {
            string output = "";
            string temp = "";
            int state = 0;
            foreach (char c in input)
            {
                switch (state)
                {
                    case 0:
                        if (c == '\\')
                        {
                            state = 1;
                            temp += c;
                        }
                        else
                            output += c;
                        break;
                    case 1:
                        if (c == 'u')
                        {
                            state = 2;
                            temp += c;
                        }
                        else
                        {
                            state = 0;
                            output += temp + c;
                            temp = "";
                        }
                        break;
                    case 2:
                        if (temp.Length < 5)
                        {
                            state = 2;
                            temp += c;
                        }
                        else
                        {
                            try
                            {
                                state = 0;
                                temp += c;
                                int code = int.Parse(temp.Substring(2), System.Globalization.NumberStyles.HexNumber);
                                if (code == 0x9 || code == 0xa || code == 0xd || (code >= 0x20 && code <= 0xd7ff) || (code >= 0xe000 && code <= 0xfffd))
                                    output += temp;
                            }
                            catch
                            {
                                output += temp;
                            }
                            temp = "";
                        }
                        break;
                }
            }


            return output;
        }

        XmlDocument GetAsyncAll(string name, string endPoint, string args)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode e = xmlDoc.CreateNode(XmlNodeType.Element, name, "");
            xmlDoc.AppendChild(e);

            var getTask = GetAsync(endPoint, args);
            Task.WaitAll(getTask);

            XmlDocument xmlDocument = getTask.Result;
            XmlNode importedDocument = xmlDoc.ImportNode(xmlDocument.DocumentElement, true);
            xmlDoc.DocumentElement.AppendChild(importedDocument);

            XmlNode paging = GetNode(xmlDoc.FirstChild.FirstChild.ChildNodes, "paging");

            if (paging != null)
            {
                XmlNode next = GetNode(paging.ChildNodes, "next");

                while (next != null)
                {
                    XmlNode after = GetNode(GetNode(paging.ChildNodes, "cursors").ChildNodes, "after");
                    getTask = GetAsync(endPoint, args + "&after=" + after.InnerText);
                    Task.WaitAll(getTask);
                    xmlDocument = getTask.Result;
                    importedDocument = xmlDoc.ImportNode(xmlDocument.DocumentElement, true);
                    xmlDoc.DocumentElement.AppendChild(importedDocument);

                    paging = GetNode(xmlDoc.FirstChild.LastChild.ChildNodes, "paging");
                    next = GetNode(paging.ChildNodes, "next");
                }
            }
            return xmlDoc;
        }

        XmlDocument GetAsyncAllII(string name, string endPoint, string args)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode e = xmlDoc.CreateNode(XmlNodeType.Element, name, "");
            xmlDoc.AppendChild(e);

            var getTask = GetAsync(endPoint, args);
            Task.WaitAll(getTask);

            XmlDocument xmlDocument = getTask.Result;
            XmlNode importedDocument = xmlDoc.ImportNode(xmlDocument.DocumentElement, true);
            xmlDoc.DocumentElement.AppendChild(importedDocument);

            XmlNode paging = GetNode(xmlDoc.FirstChild.FirstChild.ChildNodes, "paging");

            while (paging != null)
            {
                XmlNode next = GetNode(paging.ChildNodes, "next");
                string s = next.InnerText;
                int pos = s.IndexOf("?");
                s = s.Substring(pos + 1);
                getTask = GetAsync(endPoint, s);
                Task.WaitAll(getTask);
                xmlDocument = getTask.Result;
                importedDocument = xmlDoc.ImportNode(xmlDocument.DocumentElement, true);
                xmlDoc.DocumentElement.AppendChild(importedDocument);

                paging = GetNode(xmlDoc.FirstChild.LastChild.ChildNodes, "paging");
            }
            return xmlDoc;
        }

        async Task<XmlDocument> GetAsync(string endPoint, string args)
        {
            var result = await facebookClient.GetAsync(UpdateStatus, accessToken, endPoint, args);
            string res;
            if (result == null)
                res = "{}";
            else
                res = result;
            res = CleanXml("{page:," + res.Substring(1));
            XmlDocument xmlDocument = JsonConvert.DeserializeXmlNode(res);

            return xmlDocument;
        }

        void UpdateStatus(string s)
        {
            Invoke(new Action(() => textBox3.AppendText(s)));

        }

        void ClearAll()
        {
            Invoke(new Action(() =>
            {
                selectedGroup = -1;
                groupList.Clear();
                comboBox1.Items.Clear();
                comboBox1.Text = "";
                ClearAlbums();
            }));
        }

        void ClearAlbums()
        {
            Invoke(new Action(() =>
            {
                selectedAlbum = -1;
                albumList.Clear();
                albumSubList.Clear();
                comboBox2.Items.Clear();
                comboBox2.Text = "";
            }));
        }

        void DisableAll()
        {
            Invoke(new Action(() =>
            {
                button3.BackColor = Color.Red;
                button3.Text = "Wait";
                foreach (Control c in Controls)
                    c.Enabled = false;
                textBox3.Enabled = true;
            }));
        }

        void EnableAll()
        {
            Invoke(new Action(() =>
            {
                button3.BackColor = Color.Green;
                button3.Text = "";
                foreach (Control c in Controls)
                    c.Enabled = true;
            }));
        }

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("cputime.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long cputime();

    }
}
