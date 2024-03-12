using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RougeGenerator
{
    const int MIN_ROOM_SIZE = 4;
    const int MAX_ROOM_SIZE = 8;
    const int MIN_SPACE_BETWEEN_ROOM_AND_ROAD = 2;
    int width, height;
    int[,] map;
    List<Area> areaList;

    public RougeGenerator(int width, int height)
    {
        this.width = width;
        this.height = height;

        map = new int[this.width, this.height];
    }

    public int[,] GenerateMap()
    {
        areaList = new List<Area>();
        InitMap();
        InitFirstArea();
        
        DivideArea(Random.Range(0, 2) == 0);

        CreateRoom();

        ConnectRooms();

        return map;
    }
    void InitFirstArea()
    {
        Area area = new Area();
        area.Section.SetPoints(0, 0, width - 1, height - 1);
        areaList.Add(area);
    }

    void InitMap()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[x, y] = (int)CellType.Wall;
            }
        }
    }

    /*�����𕪊����Ă���������*/
    void DivideArea(bool horizontalDivide)
    {
        Area parentArea = areaList.LastOrDefault();
        if (parentArea == null) return;

        areaList.Remove(parentArea);
        
       // horizontalDivide = true;
        Area childArea = horizontalDivide ? DivideHorizontally(parentArea) : DivideVertially(parentArea);

        if (childArea != null)
        {
            DrawBorder(parentArea);
            DrawBorder(childArea);

            if(parentArea.Section.Size > childArea.Section.Size)
            {
                areaList.Add(childArea);
                areaList.Add(parentArea);
            }
            else
            {
                areaList.Add(parentArea);
                areaList.Add(childArea);
            }
            DivideArea(!horizontalDivide);
           
        }
    }

    //��������
    Area DivideHorizontally(Area area)
    {
        area.DivideDirection = DivideDirection.Horizontal;
        if (!CheckRectSize(area.Section.Height)) {
            areaList.Add(area);
            return null;
        };

        int divideLine = CalculateDivideLine(area.Section.Top,area.Section.Bottom);
        Area childArea = new Area();

        //�q���ݒ�
        childArea.Section.SetPoints(area.Section.Left, divideLine, area.Section.Right, area.Section.Bottom);
        //�e���X�V�i���ӂ𕪊����C���ɐݒ�j
        area.Section.Bottom = divideLine;
        return childArea;
    }

    //��������
    Area DivideVertially(Area area)
    {
        area.DivideDirection = DivideDirection.Vertical;
        if (!CheckRectSize(area.Section.Width))
        {
            areaList.Add(area);
            return null;
        };

        int divideLine = CalculateDivideLine(area.Section.Left, area.Section.Right);
        Area childArea = new Area();

        childArea.Section.SetPoints(divideLine, area.Section.Top, area.Section.Right, area.Section.Bottom);
        area.Section.Right = divideLine;
        return childArea;
    }

    //�Z�N�V�����T�C�Y�`�F�b�N
    bool CheckRectSize(int size)
    {
        //�ŏ��̕����T�C�Y�{���̃}�[�W����*2�i2�������邽�߁j�{1�i�����j
        int min = (MIN_ROOM_SIZE + MIN_SPACE_BETWEEN_ROOM_AND_ROAD) * 2 + 1;
        return size >= min;
    }

    //�������C���v�Z
    int CalculateDivideLine(int start,int end)
    {
        int min = start + (MIN_ROOM_SIZE + MIN_SPACE_BETWEEN_ROOM_AND_ROAD);
        int max = end - (MIN_ROOM_SIZE + MIN_SPACE_BETWEEN_ROOM_AND_ROAD);

        return Random.Range(min, max + 1);
    }

    //���E���������@�i����̓e�X�g�p�j
    void DrawBorder(Area area)
    {

        for (int y = area.Section.Top; y <= area.Section.Bottom; y++)
        {
            for (int x = area.Section.Left; x <= area.Section.Right; x++)
            {
                if (x == area.Section.Left || x == area.Section.Right || y == area.Section.Top || y == area.Section.Bottom)
                {
                    map[x, y] = (int)CellType.BorderLine;
                }
            }
        }
    }
    /*�����𕪊����Ă������������܂Ł�*/

    /*���������쐬���鏈����*/
    void CreateRoom()
    {
        foreach (Area area in areaList)
        {
            CreateRoomInArea(area);
        }
    }
    void CreateRoomInArea(Area area)
    {
        // �����̊�{�I�ȍ��ӂƉE�ӂ̈ʒu���v�Z
        int roomLeft = area.Section.Left + MIN_SPACE_BETWEEN_ROOM_AND_ROAD;
        int roomRight = area.Section.Right - MIN_SPACE_BETWEEN_ROOM_AND_ROAD + 1;
        // �����̍��ӂƉE�ӂ̈ʒu�������_���ɒ���
        AdjustRoomSidePosition(ref roomLeft,ref roomRight);
 
        // ���l�ɏ�ӂƉ��ӂ̈ʒu���v�Z
        int roomTop = area.Section.Top + MIN_SPACE_BETWEEN_ROOM_AND_ROAD;
        int roomBottom = area.Section.Bottom - MIN_SPACE_BETWEEN_ROOM_AND_ROAD + 1;
        AdjustRoomSidePosition(ref roomTop,ref roomBottom);
        
        //area�ɕ������W���Z�b�g
        area.Room.SetPoints(roomLeft, roomTop, roomRight, roomBottom);
        // �������}�b�v�ɔz�u
        for (int y = roomTop; y < roomBottom; y++)
        {
            for (int x = roomLeft; x < roomRight; x++)
            {
                map[x, y] = (int)CellType.Path;
            }
        }
    }

    //�����|�W�V������ݒ肷��
    void AdjustRoomSidePosition(ref int minPosition,ref int maxPosition)
    {
        if(minPosition + MIN_ROOM_SIZE < maxPosition)
        {
            // �^����ꂽ�͈͓��Ń����_���ȕ����̕ӂ̈ʒu���v�Z
            int maxRange = Mathf.Min(minPosition + MAX_ROOM_SIZE, maxPosition);//�����_���̍ő�l�擾
            int minRange = minPosition + MIN_ROOM_SIZE;//�����_���̍ŏ��l�擾
            
            //Debug.Log($"minRange {minRange}/ maxRange {maxRange}");
            int position = Random.Range(minRange, maxRange + 1);//�|�W�V�����v�Z
            int diff = Random.Range(0, maxPosition - position);//���炷�l���v�Z
            //�����i�����j�ɍ��W�����炷
            minPosition += diff;
            maxPosition = position + diff;
        }
    }
    /*���������쐬���鏈�������܂Ł�*/

    /*���ʘH����鏈����*/
    void ConnectRooms()
    {
        for (int i = 0; i < areaList.Count - 1; i++)
        {
            Area parentArea = areaList[i];
            Area childArea = areaList[i + 1];
            CreateRoadBetweenAreas(parentArea, childArea);

            // ���G���A�Ƃ̐ڑ������݂�
            if (i < areaList.Count - 2)
            {
                Area grandchildArea = areaList[i + 2];
                CreateRoadBetweenAreas(parentArea, grandchildArea, true);
            }
        }
    }

    void CreateRoadBetweenAreas(Area parentArea, Area childArea, bool isGrandchild = false)
    {
        if (parentArea.Section.Bottom == childArea.Section.Top || parentArea.Section.Top == childArea.Section.Bottom)
        {
            CreateVerticalRoad(parentArea, childArea, isGrandchild);
        }
        else if (parentArea.Section.Right == childArea.Section.Left || parentArea.Section.Left == childArea.Section.Right)
        {
            CreateHorizontalRoad(parentArea, childArea, isGrandchild);
        }
        else
        {
            Debug.Log("���Ƃ̐ڑ��s�\");
        }
    }

    void CreateVerticalRoad(Area parentArea, Area childArea, bool isGrandchild)
    {
        int xStart = isGrandchild && parentArea.Road != null ? parentArea.Road.Left : Random.Range(parentArea.Room.Left, parentArea.Room.Right);
        int xEnd = isGrandchild && childArea.Road != null ? childArea.Road.Left : Random.Range(childArea.Room.Left, childArea.Room.Right);
        int connectY = parentArea.Section.Bottom == childArea.Section.Top ? childArea.Section.Top : parentArea.Section.Top;
        
        //��������ڑ������܂œ������
        if (parentArea.Section.Top > childArea.Section.Top)
        {
            parentArea.SetRoad(xStart, connectY, xStart + 1, parentArea.Room.Top);
            childArea.SetRoad(xEnd, childArea.Room.Bottom, xEnd + 1, connectY);
        }
        else
        {
            parentArea.SetRoad(xStart, parentArea.Room.Bottom, xStart + 1, connectY);
            childArea.SetRoad(xEnd, connectY, xEnd + 1, childArea.Room.Top);
        }
        //��������ڑ������܂ł𓹂ɂ���
        DrawRoadFromRoomToConnectLine(parentArea);
        DrawRoadFromRoomToConnectLine(childArea);

        //�ڑ������𓹂ɂ���
        DrawVerticalRoad(xStart, xEnd, connectY);
    }

    void CreateHorizontalRoad(Area parentArea, Area childArea, bool isGrandchild)
    {
        int yStart = isGrandchild && parentArea.Road != null ? parentArea.Road.Top : Random.Range(parentArea.Room.Top, parentArea.Room.Bottom);
        int yEnd = isGrandchild && childArea.Road != null ? childArea.Road.Top : Random.Range(childArea.Room.Top, childArea.Room.Bottom);
        int connectX = parentArea.Section.Right == childArea.Section.Left ? childArea.Section.Left : parentArea.Section.Left;
        if (parentArea.Section.Left > childArea.Section.Left)
        {
            //��������ڑ������܂œ������
            parentArea.SetRoad(connectX, yStart, parentArea.Room.Left, yStart + 1);
            childArea.SetRoad(childArea.Room.Right, yEnd, connectX, yEnd + 1);
        }
        else
        {
            connectX = childArea.Section.Left;
            parentArea.SetRoad(parentArea.Room.Right, yStart, connectX, yStart + 1);
            childArea.SetRoad(connectX, yEnd, childArea.Room.Left, yEnd + 1);
        }
        //��������ڑ������܂ł𓹂ɂ���
        DrawRoadFromRoomToConnectLine(parentArea);
        DrawRoadFromRoomToConnectLine(childArea);
        //�ڑ������𓹂ɂ���
        DrawHorizontalRoad(yStart, yEnd, connectX);
    }
    void DrawRoadFromRoomToConnectLine(Area area)
    {
        for (int y = 0; y < area.Road.Height; y++)
        {
            for (int x = 0; x < area.Road.Width; x++)
            {
                map[x + area.Road.Left, y + area.Road.Top] = (int)CellType.Path;
            }
        }
    }

    void DrawVerticalRoad(int xStart, int xEnd, int y)
    {
        for (int x = Mathf.Min(xStart, xEnd); x <= Mathf.Max(xStart, xEnd); x++)
        {
            map[x, y] = (int)CellType.Path;
        }
    }

    void DrawHorizontalRoad(int yStart, int yEnd, int x)
    {
        for (int y = Mathf.Min(yStart, yEnd); y <= Mathf.Max(yStart, yEnd); y++)
        {
            map[x, y] = (int)CellType.Path;
        }
    }


    string DumpRect(Rect rect)
    {
        return $"left:{rect.Left} , top:{rect.Top} , bottom:{rect.Bottom} , Right:{rect.Right} , Wdith:{rect.Width} , Height:{rect.Height}";
    }
}



//���A�����A�ʘH�Ȃǂ̋�`�p�̃N���X
public class Rect
{
    int top = 0;
    int right = 0;
    int bottom = 0;
    int left = 0;

    public int Top
    {
        get { return top; }
        set { top = value; }
    }
    public int Right
    {
        get { return right; }
        set { right = value; }
    }
    public int Bottom
    {
        get { return bottom; }
        set { bottom = value; }
    }
    public int Left
    {
        get { return left; }
        set { left = value; }
    }
    //���̉����i�E���獶�������j
    public int Width { get => right - left; }
    //���̍����i���������Ђ��j
    public int Height { get => bottom - top; }

    public int Size { get => Width * Height; }
    public Rect(int left = 0, int top = 0, int right = 0, int bottom = 0)
    {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }
    public void SetPoints(int left, int top, int right, int bottom)
    {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }
}

public class Area
{
    private Rect section;
    private Rect room;
    private Rect road;
    private DivideDirection divideDirection;
    public Rect Section { get => section; set => section = value; }
    public Rect Room { get => room; set => room = value; }
    public Rect Road { get => road; set => road = value; }
    public DivideDirection DivideDirection { get => divideDirection; set => divideDirection = value; }
    public Area()
    {
        section = new Rect();
        room = new Rect();
        road = null;
        divideDirection = DivideDirection.Horizontal;
    }
    public void SetRoad(int left, int top, int right, int bottom)
    {
        road = new Rect(left, top, right, bottom);
    }
}

public enum DivideDirection
{
    Vertical,
    Horizontal
}
public enum CellType
{
    Path,
    Wall,
    BorderLine
}
